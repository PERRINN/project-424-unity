using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;

namespace Perrinn424.LiveLink
{
    // Main-thread history; a bounded worker queue keeps filesystem IO off the physics thread.
    public sealed class SessionRecording : IDisposable
    {
        public sealed class Sample
        {
            public int sequence, frame;
            public double time, simulationTime;
            public bool paused;
            public float[] values;
            public string ToJson()
            {
                var b = new StringBuilder(400);
                b.Append("{\"sequence\":").Append(sequence).Append(",\"frame\":").Append(frame)
                    .Append(",\"time\":").Append(time.ToString("R", CultureInfo.InvariantCulture))
                    .Append(",\"simulationTime\":").Append(simulationTime.ToString("R", CultureInfo.InvariantCulture))
                    .Append(",\"paused\":").Append(paused ? "true" : "false").Append(",\"values\":[");
                for (int i = 0; i < values.Length; i++)
                {
                    if (i > 0) b.Append(',');
                    b.Append(float.IsNaN(values[i]) || float.IsInfinity(values[i]) ? "null" : values[i].ToString("R", CultureInfo.InvariantCulture));
                }
                return b.Append("]}").ToString();
            }
        }
        sealed class Write { public Sample sample; public string change; }
        readonly List<Sample> samples = new List<Sample>();
        readonly List<string> changes = new List<string>();
        readonly BlockingCollection<Write> writes = new BlockingCollection<Write>(4096);
        readonly Thread worker;
        readonly string metadata;
        readonly int channelCount;
        public readonly string DirectoryPath;
        public volatile string Error;
        public int Count => samples.Count;
        public int EventCount => changes.Count;
        public SessionRecording(string directory, string metadata, int channelCount = 10)
        {
            if (channelCount < 1 || channelCount > 64) throw new ArgumentOutOfRangeException(nameof(channelCount));
            this.channelCount = channelCount;
            DirectoryPath = directory; this.metadata = metadata;
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "session.json"), metadata);
            worker = new Thread(WriteFiles) { IsBackground = true, Name = "424 Telemetry recording" };
            worker.Start();
        }
        public void Append(Sample sample)
        {
            if (sample.values == null || sample.values.Length != channelCount) throw new ArgumentException("Sample channel count differs from recording header");
            sample.sequence = samples.Count + 1;
            samples.Add(sample);
            Enqueue(new Write { sample = sample });
        }
        public void AddChange(string json) { changes.Add(json); Enqueue(new Write { change = json }); }
        void Enqueue(Write write)
        {
            if (Error == null && !writes.TryAdd(write)) Error = "Disk recorder queue overflowed; the disk recording is incomplete. Browser history remains available.";
        }
        public string History(int after, int eventAfter, int limit)
        {
            var b = new StringBuilder(200000);
            b.Append("{\"type\":\"history\",\"total\":").Append(Count).Append(",\"samples\":[");
            int end = Math.Min(Count, after + Math.Max(1, Math.Min(500, limit)));
            for (int i = after; i < end; i++) { if (i > after) b.Append(','); b.Append(samples[i].ToJson()); }
            b.Append("],\"events\":[");
            int eventEnd = Math.Min(EventCount, eventAfter + 100);
            for (int i = eventAfter; i < eventEnd; i++) { if (i > eventAfter) b.Append(','); b.Append(changes[i]); }
            return b.Append("],\"eventTotal\":").Append(EventCount).Append('}').ToString();
        }
        void WriteFiles()
        {
            try
            {
                using (var binary = new BinaryWriter(new FileStream(Path.Combine(DirectoryPath, "samples.bin"), FileMode.Create, FileAccess.Write, FileShare.Read, 65536)))
                using (var events = new StreamWriter(new FileStream(Path.Combine(DirectoryPath, "changes.jsonl"), FileMode.Create, FileAccess.Write, FileShare.Read)))
                {
                    // Little endian: ULL2 + channel count; each row has 25 header bytes plus 4 per channel.
                    binary.Write(Encoding.ASCII.GetBytes("ULL2")); binary.Write(channelCount);
                    int unflushed = 0;
                    while (!writes.IsCompleted)
                    {
                        Write write;
                        if (writes.TryTake(out write, 1000))
                        {
                            if (write.sample != null)
                            {
                                var s = write.sample;
                                binary.Write(s.sequence); binary.Write(s.frame); binary.Write(s.time); binary.Write(s.simulationTime); binary.Write(s.paused);
                                foreach (var value in s.values) binary.Write(value);
                            }
                            else events.WriteLine(write.change);
                            unflushed++;
                        }
                        else unflushed = 50;
                        if (unflushed >= 50) { binary.Flush(); events.Flush(); unflushed = 0; }
                    }
                }
            }
            catch (Exception ex) { Error = "Disk recording failed: " + ex.Message; }
        }
        public void Dispose()
        {
            writes.CompleteAdding();
            if (!worker.Join(5000)) Error = "Disk recording is still flushing; do not exit Unity yet.";
            if (Error == null)
            {
                try
                {
                    string completed = metadata.Replace("\"complete\":false", "\"complete\":true");
                    completed = Regex.Replace(completed, "\"recordedSamples\":\\d+", "\"recordedSamples\":" + Count);
                    completed = Regex.Replace(completed, "\"recordedEvents\":\\d+", "\"recordedEvents\":" + EventCount);
                    File.WriteAllText(Path.Combine(DirectoryPath, "session.json"), completed);
                }
                catch (Exception ex) { Error = "Cannot finalize recording metadata: " + ex.Message; }
            }
        }
    }
}
