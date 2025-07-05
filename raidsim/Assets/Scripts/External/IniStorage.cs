// Downloaded from: https://github.com/illa4257/unity-IniStorage
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace dev.illa4257
{
    public class IniStorage
    {
        public string Path;
        private readonly List<Pair> pairs = new();
        private readonly List<Group> groups = new();
        private readonly List<string> endComments = new();

        public IniStorage() { }
        public IniStorage(string path) => Load(path);

        public int PairsCount
        {
            get
            {
                lock (pairs)
                    return pairs.Count;
            }
        }

        public bool Contains(Pair p)
        {
            lock (pairs)
                return pairs.Contains(p);
        }

        public bool Contains(string key)
        {
            lock (pairs)
                foreach (Pair p in pairs)
                    if (p.Name == key)
                        return true;
            return false;
        }

        public bool Contains(string group, Pair p)
        {
            if (group == null || group.Length == 0)
                return Contains(p);
            Group g = GetGroup(group);
            return g != null ? g.Contains(p) : false;
        }

        public bool Contains(string group, string key)
        {
            if (group == null || group.Length == 0)
                return Contains(key);
            Group g = GetGroup(group);
            return g != null ? g.Contains(key) : false;
        }

        public bool ContainsGroup(string group)
        {
            lock (groups)
                foreach (Group g in groups)
                    if (g.Name == group)
                        return true;
            return false;
        }

        public Pair Get(string key)
        {
            lock (pairs)
            {
                foreach (Pair p in pairs)
                    if (p.Name == key)
                        return p;
                return null;
            }
        }

        public Pair Get(string group, string key)
        {
            if (group == null || group.Length == 0)
                return Get(key);
            return GetGroup(group)?.Get(key);
        }
        public string GetString(string key) => Get(key)?.Get();

        public string GetString(string group, string key)
        {
            if (group == null || group.Length == 0)
                return GetString(key);
            return Get(group, key)?.Get();
        }

        public string GetStringOrDefault(string key, string defaultValue)
        {
            Pair p = Get(key);
            return p != null ? p.Get() : defaultValue;
        }

        public string GetStringOrDefault(string group, string key, string defaultValue)
        {
            if (group == null || group.Length == 0)
                return GetStringOrDefault(key, defaultValue);
            Pair p = Get(group, key);
            return p != null ? p.Get() : defaultValue;
        }

        public int GetInt(string key)
        {
            Pair p = Get(key);
            if (p != null)
                return p.GetInt();
            throw new IndexOutOfRangeException();
        }

        public int GetInt(string group, string key)
        {
            if (group == null || group.Length == 0)
                return GetInt(key);
            Pair p = Get(group, key);
            if (p != null)
                return p.GetInt();
            throw new IndexOutOfRangeException();
        }

        public int GetIntOrDefault(string key, int defaultValue)
        {
            Pair p = Get(key);
            return p != null ? p.GetInt(defaultValue) : defaultValue;
        }

        public int GetIntOrDefault(string group, string key, int defaultValue)
        {
            if (group == null || group.Length == 0)
                return GetIntOrDefault(key, defaultValue);
            Pair p = Get(group, key);
            return p != null ? p.GetInt(defaultValue) : defaultValue;
        }

        public float GetFloat(string key)
        {
            Pair p = Get(key);
            if (p != null)
                return p.GetFloat();
            throw new IndexOutOfRangeException();
        }

        public float GetFloat(string group, string key)
        {
            if (group == null || group.Length == 0)
                return GetFloat(key);
            Pair p = Get(group, key);
            if (p != null)
                return p.GetFloat();
            throw new IndexOutOfRangeException();
        }

        public float GetFloatOrDefault(string key, float defaultValue)
        {
            Pair p = Get(key);
            return p != null ? p.GetFloat(defaultValue) : defaultValue;
        }

        public float GetFloatOrDefault(string group, string key, float defaultValue)
        {
            if (group == null || group.Length == 0)
                return GetFloatOrDefault(key, defaultValue);
            Pair p = Get(group, key);
            return p != null ? p.GetFloat(defaultValue) : defaultValue;
        }

        public double GetDouble(string key)
        {
            Pair p = Get(key);
            if (p != null)
                return p.GetDouble();
            throw new IndexOutOfRangeException();
        }

        public double GetDouble(string group, string key)
        {
            if (group == null || group.Length == 0)
                return GetDouble(key);
            Pair p = Get(group, key);
            if (p != null)
                return p.GetDouble();
            throw new IndexOutOfRangeException();
        }

        public double GetDoubleOrDefault(string key, double defaultValue)
        {
            Pair p = Get(key);
            return p != null ? p.GetDouble(defaultValue) : defaultValue;
        }

        public double GetDoubleOrDefault(string group, string key, double defaultValue)
        {
            if (group == null || group.Length == 0)
                return GetDoubleOrDefault(key, defaultValue);
            Pair p = Get(group, key);
            return p != null ? p.GetDouble(defaultValue) : defaultValue;
        }

        public Group GetGroup(string group)
        {
            lock (groups)
                foreach (Group g in groups)
                    if (g.Name == group)
                        return g;
            return null;
        }

        public Pair Set(Pair p)
        {
            lock (pairs)
                foreach (Pair pair in pairs)
                    if (pair.Name == p.Name)
                    {
                        pair.Set(p.Get());
                        return pair;
                    }
            pairs.Add(p);
            return p;
        }

        public Pair Set(string key, object value)
        {
            lock (pairs)
                foreach (Pair pair in pairs)
                    if (pair.Name == key)
                    {
                        pair.Set(value.ToString());
                        return pair;
                    }
            return Set(new(key, value.ToString()));
        }

        public Pair Set(string group, Pair p)
        {
            if (group == null || group.Length == 0)
                return Set(p);
            Group g = GetGroup(group);
            if (g == null)
            {
                g = new Group(group);
                lock (groups)
                    groups.Add(g);
            }
            return g.Set(p);
        }
        public Pair Set(string group, string key, object value) => Set(group, new Pair(key, value));

        public Group ForceGroup(string name)
        {
            Group g = GetGroup(name);
            if (g == null)
                lock (groups)
                {
                    g = new Group(name);
                    groups.Add(g);
                }
            return g;
        }

        public Group NewGroup(string name)
        {
            RemoveGroup(name);
            lock (groups)
            {
                Group g = new Group(name);
                groups.Add(g);
                return g;
            }
        }

        public void Remove(Pair p)
        {
            lock (pairs)
                if (pairs.Contains(p))
                    pairs.Remove(p);
        }

        public void Remove(string key)
        {
            lock (pairs)
                foreach (Pair p in pairs)
                    if (p.Name == key)
                    {
                        pairs.Remove(p);
                        return;
                    }
        }

        public void Remove(string group, Pair p)
        {
            if (group == null || group.Length == 0)
                Remove(p);
            GetGroup(group)?.Remove(p);
        }

        public void Remove(string group, string key)
        {
            if (group == null || group.Length == 0)
                Remove(key);
            GetGroup(group)?.Remove(key);
        }

        public void RemoveGroup(string group)
        {
            lock (groups)
                foreach (Group g in groups)
                    if (g.Name == group)
                    {
                        groups.Remove(g);
                        return;
                    }
        }

        public void RemoveGroup(Group g)
        {
            lock (groups)
                if (groups.Contains(g))
                    groups.Remove(g);
        }

        public bool ContainsEndComment(string comment)
        {
            lock (endComments)
                return endComments.Contains(comment);
        }

        public int IndexOfEndComment(string comment)
        {
            lock (endComments)
                return endComments.IndexOf(comment);
        }

        public int IndexOfEndComment(string comment, int begin)
        {
            lock (endComments)
                return endComments.IndexOf(comment, begin);
        }

        public int IndexOfEndComment(string comment, int begin, int count)
        {
            lock (endComments)
                return endComments.IndexOf(comment, begin, count);
        }

        public IniStorage AddEndComment(string comment)
        {
            lock (endComments)
                endComments.Add(comment);
            return this;
        }

        public IniStorage ClearEndComments()
        {
            lock (endComments)
                endComments.Clear();
            return this;
        }

        public int EndCommentsCount
        {
            get
            {
                lock (endComments)
                    return endComments.Count;
            }
        }

        public string GetEndComment(int index)
        {
            lock (endComments)
            {
                if (index < endComments.Count)
                    return endComments[index];
                throw new IndexOutOfRangeException();
            }
        }

        public string[] GetEndComments()
        {
            lock (endComments)
            {
                return endComments.ToArray();
            }
        }

        public bool Load(string path)
        {
#if UNITY_WEBPLAYER
        if (SaveHandler.Instance != null)
        {
            string[] lines = SaveHandler.Instance.Load();
            if (lines != null && lines.Length > 0)
            {
                Load(lines);
                return true;
            }
            return false;
        }
        return false;
#else
            Path = System.IO.Path.GetFullPath(path);
            return Load();
#endif
        }

        public bool Load()
        {
            if (File.Exists(Path) && Path != null)
            {
                string[] lines;
                try
                {
                    lines = File.ReadAllLines(Path);
                }
                catch (Exception)
                {
                    return false;
                }
                Load(lines);
                return true;
            }
            else
                return false;
        }

        public void Load(StreamReader reader)
        {
            List<string> lines = new();
            string line;
            while ((line = reader.ReadLine()) != null)
                lines.Add(line);
            Load(lines.ToArray());
        }

        public void Load(string[] lines)
        {
            lock (groups)
            {
                lock (pairs)
                {
                    lock (endComments)
                    {
                        groups.Clear();
                        pairs.Clear();
                        endComments.Clear();
                        List<string> comments = new();
                        Group g = null;
                        foreach (string line in lines)
                        {
                            if (line.Length == 0)
                                continue;

                            if (line.StartsWith('#'))
                            {
                                comments.Add(line[1..]);
                                continue;
                            }

                            if (line.StartsWith('[') && line.EndsWith(']'))
                            {
                                string n = line.Substring(1, line.LastIndexOf(']') - 1);
                                if (n.Length == 0)
                                {
                                    g = null;
                                    continue;
                                }
                                g = new Group(n);
                                g.AddComments(comments);
                                comments.Clear();
                                groups.Add(g);
                                continue;
                            }

                            if (line.Contains('='))
                            {
                                string n = line.Substring(0, line.IndexOf('='));
                                if (n.Length == 0)
                                    continue;
                                Pair p = new(n, line[(line.IndexOf('=') + 1)..], comments);
                                comments.Clear();
                                if (g != null)
                                    g.Set(p);
                                else
                                    Set(p);
                            }
                        }
                        if (comments.Count > 0)
                            endComments.AddRange(comments);
                    }
                }
            }
        }

        public bool Save() => Save(true, true);
        public bool Save(bool split) => Save(true, split);

        public bool Save(bool comments, bool split)
        {
#if UNITY_WEBPLAYER
        if (SaveHandler.Instance != null)
        {
            SaveHandler.Instance.Write(ToLines(comments, split));
            return true;
        }
        return false;
#else
            try
            {
                File.WriteAllLines(Path, ToLines(comments, split));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
#endif
        }

        public override string ToString() => String.Join('\n', ToLines());

        public string[] ToLines() => ToLines(true, true);
        public string[] ToLines(bool split) => ToLines(true, split);

        public string[] ToLines(bool comments, bool split)
        {
            lock (groups)
            {
                lock (pairs)
                {
                    lock (endComments)
                    {
                        List<string> content = new();
                        foreach (Pair p in pairs)
                        {
                            if (comments)
                            {
                                if (split && p.CommentsCount > 0 && content.Count > 0)
                                    content.Add("");
                                foreach (string comment in p.GetComments())
                                    content.Add('#' + comment);
                            }

                            content.Add(p.Name + '=' + p.Get());
                        }

                        foreach (Group g in groups)
                        {
                            if (comments)
                            {
                                if (split)
                                    content.Add("");
                                foreach (string comment in g.GetComments())
                                    content.Add('#' + comment);
                            }
                            content.Add('[' + g.Name + ']');
                            foreach (Pair p in g)
                            {
                                if (comments)
                                {
                                    if (split && p.CommentsCount > 0)
                                        content.Add("");
                                    foreach (string comment in p.GetComments())
                                        content.Add('#' + comment);
                                }
                                content.Add(p.Name + '=' + p.Get());
                            }
                        }

                        if (comments)
                        {
                            if (split && content.Count > 0)
                                content.Add("");
                            foreach (string comment in endComments)
                                content.Add('#' + comment);
                        }

                        return content.ToArray();
                    }
                }
            }
        }

        public class Comments
        {
            private readonly List<string> comments = new();

            public void AddComments(List<string> list)
            {
                lock (comments)
                    comments.AddRange(list);
            }

            public string[] GetComments()
            {
                lock (comments)
                    return comments.ToArray();
            }

            public bool ContainsComment(string comment)
            {
                lock (comments)
                    return comments.Contains(comment);
            }

            public int IndexOfComment(string comment)
            {
                lock (comments)
                    return comments.IndexOf(comment);
            }

            public int IndexOfComment(string comment, int begin)
            {
                lock (comments)
                    return comments.IndexOf(comment, begin);
            }

            public int IndexOfComment(string comment, int begin, int count)
            {
                lock (comments)
                    return comments.IndexOf(comment, begin, count);
            }

            public Comments AddComment(string comment)
            {
                lock (comments)
                    comments.Add(comment);
                return this;
            }

            public int CommentsCount
            {
                get
                {
                    lock (comments)
                        return comments.Count;
                }
            }

            public string GetComment(int index)
            {
                lock (comments)
                {
                    if (index < comments.Count)
                        return comments[index];
                    throw new IndexOutOfRangeException();
                }
            }

            public Comments ClearComments()
            {
                lock (comments)
                    comments.Clear();
                return this;
            }
        }

        public class Pair : Comments
        {
            public string Name;
            private string value;

            public Pair Set(object v)
            {
                value = v.ToString();
                return this;
            }

            public string Get() => value;

            public int GetInt() => int.Parse(value);

            public int GetInt(int defaultValue)
            {
                if (int.TryParse(value, out int r))
                    return r;
                return defaultValue;
            }

            public float GetFloat() => float.Parse(value);

            public float GetFloat(float defaultValue)
            {
                if (float.TryParse(value, out float r))
                    return r;
                return defaultValue;
            }

            public double GetDouble() => double.Parse(value);

            public double GetDouble(double defaultValue)
            {
                if (double.TryParse(value, out double r))
                    return r;
                return defaultValue;
            }

            public Pair(string n, object v)
            {
                Name = n;
                Set(v);
            }

            public Pair(string n, object v, List<string> comments)
            {
                AddComments(comments);
                Name = n;
                Set(v);
            }
        }

        public class Group : Comments, IEnumerable<Pair>
        {
            public string Name;
            private readonly List<Pair> pairs = new();

            public Group(string name)
            {
                if (name.Length == 0)
                    throw new Exception("Minimum group name length 1");
                Name = name;
            }

            public Pair[] ToArray()
            {
                lock (pairs)
                    return pairs.ToArray();
            }

            public IEnumerator<Pair> GetEnumerator() => new PairEnumerator(ToArray());

            public int Count
            {
                get
                {
                    lock (pairs)
                        return pairs.Count;
                }
            }

            public bool Contains(string key)
            {
                lock (pairs)
                    foreach (Pair p in pairs)
                        if (p.Name == key)
                            return true;
                return false;
            }

            public bool Contains(Pair p)
            {
                lock (pairs)
                    return pairs.Contains(p);
            }

            public Pair Get(string key)
            {
                lock (pairs)
                    foreach (Pair p in pairs)
                        if (p.Name == key)
                            return p;
                return null;
            }

            public string GetString(string key) => Get(key)?.Get();

            public string GetString(string key, string defaultValue)
            {
                Pair p = Get(key);
                return p != null ? p.Get() : defaultValue;
            }

            public int GetInt(string key)
            {
                Pair p = Get(key);
                if (p != null)
                    return p.GetInt();
                throw new IndexOutOfRangeException();
            }

            public int GetInt(string key, int defaultValue)
            {
                Pair p = Get(key);
                return p != null ? p.GetInt(defaultValue) : defaultValue;
            }

            public float GetFloat(string key)
            {
                Pair p = Get(key);
                if (p != null)
                    return p.GetFloat();
                throw new IndexOutOfRangeException();
            }

            public float GetFloat(string key, float defaultValue)
            {
                Pair p = Get(key);
                return p != null ? p.GetFloat(defaultValue) : defaultValue;
            }

            public double GetDouble(string key)
            {
                Pair p = Get(key);
                if (p != null)
                    return p.GetDouble();
                throw new IndexOutOfRangeException();
            }

            public double GetDouble(string key, double defaultValue)
            {
                Pair p = Get(key);
                return p != null ? p.GetDouble(defaultValue) : defaultValue;
            }

            public Pair Set(Pair p)
            {
                lock (pairs)
                    foreach (Pair pair in pairs)
                        if (pair.Name == p.Name)
                        {
                            pair.Set(p.Get());
                            return pair;
                        }
                pairs.Add(p);
                return p;
            }

            public Pair Set(string key, object value)
            {
                lock (pairs)
                    foreach (Pair pair in pairs)
                        if (pair.Name == key)
                        {
                            pair.Set(value.ToString());
                            return pair;
                        }
                return Set(new(key, value.ToString()));
            }

            public void Remove(string key)
            {
                lock (pairs)
                    foreach (Pair p in pairs)
                        if (p.Name == key)
                        {
                            pairs.Remove(p);
                            return;
                        }
            }

            public void Remove(Pair p)
            {
                lock (pairs)
                    if (pairs.Contains(p))
                        pairs.Remove(p);
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public class PairEnumerator : IEnumerator<Pair>
            {
                private readonly Pair[] pairs;
                private int pos = -1;

                public PairEnumerator(Pair[] list) => pairs = list;

                public Pair Current
                {
                    get
                    {
                        if (pairs.Length > pos)
                            return pairs[pos];
                        throw new InvalidOperationException();
                    }
                }

                object IEnumerator.Current => Current;

                public void Dispose() { }

                public bool MoveNext()
                {
                    pos++;
                    return pos < pairs.Length;
                }

                public void Reset() => pos = -1;
            }
        }
    }
}