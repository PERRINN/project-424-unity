import importlib.util
import json
import struct
import tempfile
import unittest
from pathlib import Path

spec = importlib.util.spec_from_file_location('performance_analysis', Path(__file__).with_name('analyze-performance.py'))
analysis = importlib.util.module_from_spec(spec)
spec.loader.exec_module(analysis)


class PerformanceAnalysisTests(unittest.TestCase):
    def write_recording(self, folder, paused=False):
        (folder / 'session.json').write_text(json.dumps({'session': 'measured', 'complete': True}))
        raw = bytearray(b'ULL2' + struct.pack('<i', 1))
        sim = 0
        for t in range(22):
            pause = paused and 10 <= t <= 12
            if t and not pause:
                sim += 3.2
            raw.extend(struct.pack('<iidd?f', t + 1, t * 10, float(t), sim, pause, 0))
        (folder / 'samples.bin').write_bytes(raw)

    def test_measures_original_clocks_and_excludes_warmup(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            self.write_recording(folder)
            result = analysis.analyze(folder)
            self.assertEqual(result['averageRatio'], 3.2)
            self.assertEqual(result['medianFiveSecondRatio'], 3.2)
            self.assertEqual(result['measuredWallSeconds'], 16)
            self.assertEqual(result['samples'], 22)
            self.assertEqual(result['profile'], 'unprofiled')

    def test_pause_intervals_and_partial_live_row(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            self.write_recording(folder, paused=True)
            with (folder / 'samples.bin').open('ab') as stream:
                stream.write(b'partial')
            result = analysis.analyze(folder)
            self.assertEqual(result['averageRatio'], 3.2)
            self.assertEqual(result['samples'], 22)
            self.assertLess(result['measuredWallSeconds'], 16)

    def test_reports_profile_and_component_decisions(self):
        with tempfile.TemporaryDirectory() as directory:
            folder = Path(directory)
            self.write_recording(folder)
            (folder / 'performance.json').write_text(json.dumps({
                'profile': 'telemetry-fast-v1', 'applied': True, 'fixedDeltaTime': .002,
                'inventory': [{'type': 'Display', 'action': 'disable presentation', 'enabledCount': 1},
                              {'type': 'Physics', 'action': 'retain', 'enabledCount': 1}],
            }))
            result = analysis.analyze(folder)
            self.assertEqual(result['profile'], 'telemetry-fast-v1')
            self.assertEqual(result['fixedDeltaTime'], .002)
            self.assertEqual(len(result['disabledByType']), 1)


if __name__ == '__main__':
    unittest.main()
