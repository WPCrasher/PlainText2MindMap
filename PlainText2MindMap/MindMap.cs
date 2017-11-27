using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PlainText2MindMap
{
    class MindMap
    {
        static Dictionary<String, int> freq = new Dictionary<string, int>();
        static List<Abz> textList = new List<Abz>();

        public void build(string fileName)
        {
            var text = File.ReadAllText(fileName);
            if (String.IsNullOrEmpty(text)) return;

            int i = 0;
            foreach (var abz in Regex.Split(text, "\r\n\r\n"))
                prepareData(abz.Replace("\r\n", " "), i++);

            freq = clearObject().OrderByDescending(t => t.Value).ToDictionary(t => t.Key, t => t.Value);

            foreach (var abz in textList)
            {
                bool isEmpty = true;

                foreach (var word in abz.words)
                {
                    if (!freq.ContainsKey(word.key)) continue;

                    isEmpty = false;
                    Console.Write("[" + word.name + "] ");
                }

                if (isEmpty) continue;

                Console.WriteLine();
                Console.WriteLine();
            }
        }

        private static void prepareData(string text, int num)
        {
            List<Word> words = new List<Word>();
            foreach (var sent in Regex.Split(text, "[^a-zA-Z0-9 ]"))
            {
                if (string.IsNullOrEmpty(sent)) continue;

                List<String> tail = new List<string>();
                List<String> srcTail = new List<string>();
                string str = ""; int swcnt = 0;
                foreach (var word in Regex.Split(sent, "[^a-zA-Z0-9]"))
                {
                    string key = Lingvo.getKey(word);

                    if (String.IsNullOrEmpty(key) || Lingvo.isNotNoun(word))
                    {
                        /*
                                 str = (str + " " + word).Trim();
                                 swcnt++;
                                             if (swcnt > 2)
                                             {
                                                 tail.Clear();
                                                 srcTail.Clear();
                                                 str = "";
                                                 swcnt = 0;
                                             }
                                             */
                        continue;
                    }
                    if (srcTail.Count > 0) srcTail[srcTail.Count - 1] = (srcTail[srcTail.Count - 1] + " " + str).Trim();
                    str = ""; swcnt = 0;
                    srcTail.Add(word);

                    while (tail.Count() > 0) tail.Remove(tail[0]); //!!!
                    tail.Add(key);

                    string comlpexName = "";
                    string dicName = "";
                    for (int i = 1; i <= tail.Count; i++)
                    {
                        comlpexName = tail[tail.Count - i] + " " + comlpexName;
                        comlpexName = comlpexName.Trim();

                        string comlpexKey = "";
                        foreach (var s in comlpexName.Split(' ').OrderBy(t => t))
                            comlpexKey += s + " ";
                        comlpexKey = comlpexKey.Trim();

                        if (freq.ContainsKey(comlpexKey)) freq[comlpexKey]++;
                        else freq.Add(comlpexKey, 1);

                        dicName = srcTail[srcTail.Count - i] + " " + dicName;
                        dicName = dicName.Trim();

                        if (!words.Where(k => k.name == dicName).Any())
                            words.Add(new Word()
                            {
                                key = comlpexKey,
                                name = dicName
                            });
                        /*
                        if (!XWord.Contains(comlpexKey)) XWord.Add(comlpexKey);
                                    int xid = XWord.IndexOf(comlpexKey);
                                    if (!XRel[num].Contains(xid)) XRel[num].Add(xid);
                                    */


                        //if (!xray[num].Contains(comlpexKey)) xray[num].Add(comlpexKey);
                    }
                }
            }

            textList.Add(new Abz() { num = num, words = words });
        }

        private static Dictionary<string, int> clearObject()
        {
            //Зачищаем лишнее
            List<string> forDel = new List<string>();
            Dictionary<string, int> forChange = new Dictionary<string, int>();
            foreach (var dic in freq)
                if (dic.Value < 2) forDel.Add(dic.Key);
            foreach (var d in forDel) freq.Remove(d);
            forDel.Clear();

            foreach (var dic in freq)
            {
                var d = dic.Key.Split(' ');

                var sel =
                  from f in freq
                  where f.Value * 100 >= dic.Value && !dic.Key.Equals(f.Key) && (f.Key.Contains(dic.Key) || d.All(s => f.Key.Split(' ').Contains(s)))
                  select f;

                if (sel.Count() > 0)
                {
                    foreach (var s in sel)
                    {
                        int val = Math.Max(0, dic.Value - s.Value);
                        if (val < 0) continue;
                        if (forChange.ContainsKey(s.Key)) forChange[s.Key] += val;
                        else forChange.Add(s.Key, val);
                    }
                    forDel.Add(dic.Key);
                }
            }
            foreach (var d in forDel) freq.Remove(d);

            foreach (var d in forChange)
                if (freq.ContainsKey(d.Key))
                    freq[d.Key] += d.Value;

            return freq;
        }

        private static string toSentCase(string c)
        {
            return c[0].ToString().ToUpper() + c.Substring(1);
        }

    }

    class Word
    {
        public string key;
        public string name;
    }

    class NSBase
    {
        public int lev;
    }

    class Abz : NSBase
    {
        public int num;
        public List<Word> words;

        public Abz()
        {
            lev = 0;
        }
    }

    class NS : NSBase
    {
        public NSBase parent;

        public NS(NSBase _parent)
        {
            lev = parent.lev + 1;
            parent = _parent;
        }
    }
}