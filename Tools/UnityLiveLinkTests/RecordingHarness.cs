using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Perrinn424.LiveLink;
class RecordingHarness
{
    static void Main()
    {
        string path = "/tmp/perrinn-live-link/30-minute-recording";
        if (Directory.Exists(path)) Directory.Delete(path, true);
        var timer = Stopwatch.StartNew();
        string metadata = "{\"protocol\":2,\"session\":\"recorded-test\",\"complete\":false,\"recordedSamples\":0,\"recordedEvents\":0,\"rateHz\":50,\"channels\":[";
        for (int c = 0; c < 10; c++) { if (c > 0) metadata += ","; metadata += "{\"id\":\"Channel" + c + "\",\"unit\":\"test\",\"sourceHz\":50,\"available\":true}"; }
        metadata += "],\"parameters\":[{\"id\":\"frontSpringRate\",\"label\":\"Front spring rate\",\"unit\":\"N/m\",\"value\":200000,\"min\":10000,\"max\":1000000,\"available\":true}]}";
        var recorder = new SessionRecording(path, metadata);
        for (int i = 1; i <= 90000; i++)
        {
            var values = new float[10];
            for (int c = 0; c < 10; c++) values[c] = (float)(Math.Sin(i * .002 + c) * (c + 1));
            if (i == 12345) values[0] = 1000;
            if (i >= 20000 && i < 20020) values[2] = float.NaN;
            recorder.Append(new SessionRecording.Sample { frame = i * 10, time = i / 50.0 + (i >= 45000 ? 2 : 0), simulationTime = i / 50.0, paused = false, values = values });
            if (i % 15000 == 0) recorder.AddChange("{\"id\":" + (i / 15000) + ",\"time\":" + (i / 50.0 + (i >= 45000 ? 2 : 0)) + ",\"sequence\":" + i + ",\"result\":{\"parameter\":\"frontSpringRate\",\"value\":" + (200000 + i) + ",\"effectiveFromFrame\":" + (i * 10) + "}}");
            if (i % 100 == 0) Thread.Sleep(2); // Accelerated producer, still give the asynchronous IO worker time to drain.
        }
        using (var json = new StreamWriter("/tmp/perrinn-live-link/30-minute-history.jsonl"))
            for (int after = 0; after < recorder.Count; after += 500) json.WriteLine(recorder.History(after, after == 0 ? 0 : recorder.EventCount, 500));
        recorder.Dispose();
        if (recorder.Error != null) throw new Exception(recorder.Error);
        using (var reader = new BinaryReader(File.OpenRead(Path.Combine(path, "samples.bin"))))
        {
            if (new string(reader.ReadChars(4)) != "ULL2" || reader.ReadInt32() != 10) throw new Exception("Invalid header");
            for (int i = 1; i <= 90000; i++)
            {
                if (reader.ReadInt32() != i || reader.ReadInt32() != i * 10) throw new Exception("Lost sample");
                double time = reader.ReadDouble(), simulation = reader.ReadDouble(); reader.ReadBoolean();
                if (Math.Abs(simulation - i / 50.0) > 1e-8 || Math.Abs(time - (i / 50.0 + (i >= 45000 ? 2 : 0))) > 1e-8) throw new Exception("Timestamp changed");
                for (int c = 0; c < 10; c++) { float v = reader.ReadSingle(); if (i == 12345 && c == 0 && v != 1000) throw new Exception("Spike lost"); if (i == 20000 && c == 2 && !float.IsNaN(v)) throw new Exception("Null lost"); }
            }
            if (reader.BaseStream.Position != reader.BaseStream.Length) throw new Exception("Unexpected bytes");
        }
        if (File.ReadAllLines(Path.Combine(path, "changes.jsonl")).Length != 6) throw new Exception("Lost changes");
        Console.WriteLine("PASS: 90000 recorded rows, binary round-trip, missing values, six changes, complete close flush. samples.bin = " + new FileInfo(Path.Combine(path, "samples.bin")).Length + " bytes; accelerated recording + history serialization " + timer.ElapsedMilliseconds + " ms");
    }
}
