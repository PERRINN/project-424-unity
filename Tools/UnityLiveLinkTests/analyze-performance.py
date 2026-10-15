#!/usr/bin/env python3
"""Compare actual simulation speed from local recordings; no Unity/UI profiler overhead."""
import argparse
import json
import math
import statistics
import struct
from pathlib import Path


def analyze(folder, warmup=5):
    folder = Path(folder)
    metadata = json.loads((folder / 'session.json').read_text())
    raw = (folder / 'samples.bin').read_bytes()
    if raw[:4] != b'ULL2' or len(raw) < 8:
        raise ValueError('Invalid recording header')
    channels = struct.unpack_from('<i', raw, 4)[0]
    if channels < 1 or channels > 64:
        raise ValueError('Invalid channel count')
    width = 25 + channels * 4
    count = (len(raw) - 8) // width  # Also permits a read-only snapshot of an active recording.
    previous = None
    origin = None
    simulated = elapsed = 0
    windows = {}
    for row in range(count):
        wall, sim, paused = struct.unpack_from('<dd?', raw, 8 + row * width + 8)
        if not math.isfinite(wall) or not math.isfinite(sim):
            continue
        if origin is None:
            origin = wall
        if previous and wall - origin >= warmup and previous[0] - origin >= warmup:
            dt, ds = wall - previous[0], sim - previous[1]
            if not paused and not previous[2] and dt > 0 and ds >= 0:
                elapsed += dt
                simulated += ds
                bucket = windows.setdefault(int((wall - origin) / 5), [0, 0])
                bucket[0] += dt
                bucket[1] += ds
        previous = wall, sim, paused
    ratios = [sim / wall for wall, sim in windows.values() if wall >= 1]
    audit_path = folder / 'performance.json'
    audit = json.loads(audit_path.read_text()) if audit_path.exists() else None
    return {
        'session': metadata.get('session'), 'complete': metadata.get('complete', False),
        'profile': audit.get('profile') if audit and audit.get('applied') else 'unprofiled',
        'samples': count, 'warmupSecondsExcluded': warmup,
        'measuredWallSeconds': round(elapsed, 3), 'simulatedSeconds': round(simulated, 3),
        'averageRatio': round(simulated / elapsed, 3) if elapsed else None,
        'medianFiveSecondRatio': round(statistics.median(ratios), 3) if ratios else None,
        'fixedDeltaTime': audit.get('fixedDeltaTime') if audit else None,
        'disabledByType': [entry for entry in audit.get('inventory', []) if entry['action'] != 'retain'] if audit else [],
    }


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('recordings', nargs='+', type=Path)
    parser.add_argument('--warmup', type=float, default=5, help='Real seconds to exclude at the start')
    args = parser.parse_args()
    if args.warmup < 0:
        parser.error('--warmup must be nonnegative')
    results = [analyze(folder, args.warmup) for folder in args.recordings]
    output = {'recordings': results}
    if len(results) == 2 and all(result['averageRatio'] for result in results):
        output['secondRunSpeedup'] = round(results[1]['averageRatio'] / results[0]['averageRatio'], 3)
    print(json.dumps(output, indent=2))
