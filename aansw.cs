using System;
using System.Collections.Generic;
using System.Collections; 
using System.Text;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AANSW
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                System.Console.WriteLine("Error, no directory");
                String nome_prog = System.IO.Path.GetFileName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                System.Console.WriteLine("Usage: {0} <directory>, es. > {0} 2009 ", nome_prog);
                Console.ReadKey();
                Environment.Exit(1);
            }

            string path = Path.Combine(Environment.CurrentDirectory, @args[0]);

            if (!System.IO.Directory.Exists(path))
                System.IO.Directory.CreateDirectory(path);

            Console.WriteLine("Scaricamento dati per l'anno {0} iniziato",args[0]);

            Caida test1 = new Caida(args[0]);
            Console.WriteLine("Scaricamento dati caida finito");
            Dimes test2 = new Dimes(args[0]);
            Console.WriteLine("Scaricamento dati dimes finito");
            Irl test3 = new Irl(args[0]);
            Console.WriteLine("Scaricamento dati irl finito");

            Decompressore test4 = new Decompressore(args[0]);
            Console.WriteLine("Decompressione finita");

            Analisi test5 = new Analisi();
            test5.analizzaDirectory(path);

            Console.WriteLine("Analisi finita");
            Console.WriteLine("Inizio mappaggio...:");

            Mappaggio test6 = new Mappaggio(args[0]);
            Console.WriteLine("Fine mappaggio.");
            Console.WriteLine("** TUTTO FINITO ** verificare i dati.");

            Console.ReadKey();
        }

        private static bool checkDirectory(string dir)
        {
            
            if (System.IO.Directory.Exists(dir)) return true;
            return false;
        }      
    }

    public class Analisi
    {
        Hashtable internet { get; set; }
        Hashtable irl { get; set; }
        Hashtable caida { get; set; }
        Hashtable dimes { get; set; }

        Hashtable blacklist; // as invalidi

        public Analisi()
        {
            internet = new Hashtable();
            irl = new Hashtable();
            caida = new Hashtable();
            dimes = new Hashtable();
            blacklist = new Hashtable();

            blacklist.Add("23456" , null);
            blacklist.Add("3130", null);
            blacklist.Add("0", null);
            blacklist.Add("4294967295", null);


            foreach (int b in Enumerable.Range(58368, 131071 - 58368 + 1).ToArray())
            {
                blacklist.Add(b.ToString(), null);
            }

            foreach (int b in Enumerable.Range(132096, 196607 - 132096 + 1).ToArray())
            {
                blacklist.Add(b.ToString(), null);
            }

            foreach (int b in Enumerable.Range(198656, 262143 - 198656 + 1).ToArray())
            {
                blacklist.Add(b.ToString(), null);
            }

            foreach (int b in Enumerable.Range(263168, 327679 - 263168 + 1).ToArray())
            {
                blacklist.Add(b.ToString(), null);
            }

            foreach (int b in Enumerable.Range(328704, 393215 - 328704 + 1).ToArray())
            {
                blacklist.Add(b.ToString(), null);
            }
        }

        public void analizzaCaida(string file_caida)
        {
            try
            {
                using (StreamReader objReader = new StreamReader(file_caida))
                {
                    string sLine;
                    while ((sLine = objReader.ReadLine()) != null)
                    {
                        if (sLine != null)
                        {

                            if (sLine[0].Equals('D'))
                            {
                                string as1;
                                string as2;
                                string[] aut_sys = sLine.Split('\t');
                                if (aut_sys[1].IndexOf("_") != -1) continue;
                                if (aut_sys[2].IndexOf("_") != -1) continue;
                                if (aut_sys[1].IndexOf(",") != -1) continue;
                                if (aut_sys[2].IndexOf(",") != -1) continue;
                                if (aut_sys[1].IndexOf(".") != -1)
                                {
                                    as1 = convertiDot(aut_sys[1]);
                                }
                                else as1 = aut_sys[1];

                                if (aut_sys[2].IndexOf(".") != -1)
                                {
                                    as2 = convertiDot(aut_sys[2]);
                                }
                                else as2 = aut_sys[2];

                                if (checkAS(as1, as2) == false) continue;

                                

                                if ((internet.ContainsKey(as1 + '\t' + as2) == false) && (internet.ContainsKey(as2 + '\t' + as1) == false))
                                {
                                    internet.Add(as1 + '\t' + as2, null);
                                }


                                if (caida.ContainsKey(as1 + '\t' + as2) == false && caida.ContainsKey(as2 + '\t' + as1) == false)
                                {
                                    caida.Add(as1 + '\t' + as2, null);
                                }

                            }
                        }
                    }
                    objReader.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read (Caida):");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Environment.Exit(1);
            }

        }

        public void analizzaDimes(string file_dimes)
        {
            try
            {
                using (StreamReader objReader = new StreamReader(file_dimes)) 
                {
                    string sLine ;
                    while ((sLine = objReader.ReadLine()) != null)
                    {
                        if (sLine != null)
                        {
                            string as1;
                            string as2;
                            string[] aut_sys = sLine.Split(',');
                  
                            as1 = aut_sys[0];
                            as2 = aut_sys[1];

                            if (checkAS(as1, as2) == false) continue;

                            /*
                            if ((internet.ContainsKey(as1 + '\t' + as2) == false) && (internet.ContainsKey(as2 + '\t' + as1) == false))
                            {
                                internet.Add(as1 + '\t' + as2, null);
                            }
                             */


                            if (dimes.ContainsKey(as1 + '\t' + as2) == false && dimes.ContainsKey(as2 + '\t' + as1) == false)
                            {
                                dimes.Add(as1 + '\t' + as2, null);
                            }
                        }
                    }
                    objReader.Close();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read (Dimes):");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Environment.Exit(1);
            }

        }


        public void analizzaIrl(string file_irl,string my_mese,string my_anno)
        {
            int count = 0;
            try
            {
                using (StreamReader objReader = new StreamReader(file_irl)) 
                {
                    string sLine = null;
                    while ((sLine = objReader.ReadLine()) != null)
                    {
                        if (sLine != null)
                        {
                            string as1;
                            string as2;

                            count = count + 1;

                            string[] aut_sys = sLine.Split('\t');

                            if (aut_sys[0].IndexOf(".") != -1)
                            {
                                as1 = convertiDot(aut_sys[0]);
                            }
                            else as1 = aut_sys[0];

                            if (aut_sys[1].IndexOf(".") != -1)
                            {
                                as2 = convertiDot(aut_sys[1]);
                            }
                            else as2 = aut_sys[1];

                            if (checkAS(as1, as2) == false) continue;

                            if (checkTimestamp(aut_sys[3], my_mese, my_anno) == false) continue;

                            if ((internet.ContainsKey(as1 + '\t' + as2) == false) && (internet.ContainsKey(as2 + '\t' + as1) == false))
                            {
                                internet.Add(as1 + '\t' + as2, null);
                            }


                            if (irl.ContainsKey(as1 + '\t' + as2) == false && irl.ContainsKey(as2 + '\t' + as1) == false)
                            {
                                irl.Add(as1 + '\t' + as2, null);
                            }
                        }
                    }
                objReader.Close();
               }

            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read: (Irl)");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Environment.Exit(1);
            }

        }

        public void analizzaDirectory(string anno)
        {
            string[] listaDir = Directory.GetDirectories(anno);
            string[] irl_file;
            string[] caida_file;
            string[] dimes_file;

            foreach (string mese in listaDir)
            {
                string[] m = mese.Split(Path.DirectorySeparatorChar);
                string nome_mese = m[m.Length - 1];
                Console.WriteLine("Analisi del mese {0} in corso", nome_mese);
                liberaMappe();

                try
                {
                    irl_file = Directory.GetFiles(mese, "links*");
                    caida_file = Directory.GetFiles(mese, "cycle*");
                    dimes_file = Directory.GetFiles(mese, "ASE*");

                    foreach (string f in irl_file)
                    {
                        string file = f.Substring(0, f.Length - 3);

                        if (File.Exists(file)) 
                        {
                            analizzaIrl(file,nome_mese,anno);
                        }
                    }

                    foreach (string f in dimes_file)
                    {
                        string file = f.Substring(0, f.Length - 3);
                        if (File.Exists(file)) 
                        {
                            analizzaDimes(file);
                        }
                    }
                    foreach (string f in caida_file)
                    {
                        string file = f.Substring(0, f.Length - 3);
                        if (File.Exists(file)) 
                        {
                            analizzaCaida(file);
                        }
                    }                     
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine("! Filetype not match");
                    Console.WriteLine(e.Message);
                }

                string year = anno.Substring(anno.Length - 4, 4);

                Console.WriteLine("Analisi del mese {0} finito", nome_mese);
                stampaInternet(nome_mese,year);
                stampaIRL(nome_mese,year);
                stampaCaida(nome_mese,year);
                stampaDimes(nome_mese, year);
                Console.WriteLine("Stampa del mese {0} finito", nome_mese);
                      
            }
                        
        }

        public void stampaInternet(string mese,string anno)
        {
            string output = Path.Combine(Environment.CurrentDirectory,"output");
            if (!System.IO.Directory.Exists(output))
                System.IO.Directory.CreateDirectory(output);
            string nome_file_out = "AS_internet_" + anno + "_" + mese + "_originale.txt";
            string file_output = Path.Combine(output, nome_file_out);
            StreamWriter stream = new StreamWriter(file_output);

            foreach (DictionaryEntry d in internet)
            {
                stream.WriteLine(d.Key.ToString());
            }
            stream.Close();
        }

        public void stampaCaida(string mese, string anno)
        {
            string output = Path.Combine(Environment.CurrentDirectory, "output");
            if (!System.IO.Directory.Exists(output))
                System.IO.Directory.CreateDirectory(output);
            string file_output = Path.Combine(output, "AS_caida_" + anno + "_" + mese + "_originale.txt");
            StreamWriter stream = new StreamWriter(file_output);
            foreach (DictionaryEntry d in caida)
            {
                stream.WriteLine(d.Key.ToString());
            }
            stream.Close();
        }

        public void stampaDimes(string mese, string anno)
        {
            string output = Path.Combine(Environment.CurrentDirectory, "output");
            if (!System.IO.Directory.Exists(output))
                System.IO.Directory.CreateDirectory(output);
            string file_output = Path.Combine(output, "AS_dimes_" + anno + "_" + mese + "_originale.txt");
            StreamWriter stream = new StreamWriter(file_output);
            foreach (DictionaryEntry d in dimes)
            {
                stream.WriteLine(d.Key.ToString());
            }
            stream.Close();
        }

        public void stampaIRL(string mese,string anno)
        {
            string output = Path.Combine(Environment.CurrentDirectory, "output");
            if (!System.IO.Directory.Exists(output))
                System.IO.Directory.CreateDirectory(output);
            string file_output = Path.Combine(output, "AS_irl_" + anno + "_" + mese + "_originale.txt");
            StreamWriter stream = new StreamWriter(file_output);
            foreach (DictionaryEntry d in irl)
            {
                stream.WriteLine(d.Key.ToString());
            }
            stream.Close();
        }

        private string convertiDot(string a_s)
        {
            UInt32 as_normalizzato = 0;
            try
            {
                string[] as_int = a_s.Split('.');
                Int32 prima_parte = Int32.Parse(as_int[0]);
                Int32 seconda_parte = Int32.Parse(as_int[1]);
                UInt32 prima_parte_b = (UInt32)prima_parte * 65536;
                as_normalizzato = prima_parte_b + (UInt32)seconda_parte;                
            }
            catch (OverflowException e)
            {
                Console.WriteLine("The string could not be converted: (Irl)");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.Source);
                Environment.Exit(1);
            }
            return as_normalizzato.ToString();
        }

        private bool checkAS(string as1, string as2)
        {
            if (blacklist.ContainsKey(as1)) return false;
            if (UInt32.Parse(as1) >= 394240) return false;
            if (UInt32.Parse(as2) >= 394240) return false;
            if (blacklist.ContainsKey(as2)) return false;
            return true;
        }
        
        private bool checkTimestamp(string m_timestamp, string m_mese, string m_anno)
        {
            m_anno = m_anno.Substring(m_anno.Length - 4, 4);
            int my_mese;
            int my_anno;
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            double timestamp = double.Parse(m_timestamp);
            origin = origin.AddSeconds(timestamp);
            my_mese = origin.Month;
            my_anno = origin.Year;
            int mese = Int32.Parse(m_mese);
            int anno = Int32.Parse(m_anno);            

            if (my_mese == mese && my_anno == anno)
                return true;
            return false;
        }

        private void liberaMappe()
        {
            this.internet.Clear();
            this.caida.Clear();
            this.irl.Clear();
            this.dimes.Clear();
        }

    }

    public class Caida
    {
        private ArrayList dati;

        public Caida() : this("2009") {}

        public Caida(string my_year)
        {
            string[] team = new string[3];
            string regexp = "<a href=\"" + my_year + "+.*\">(?<anno>.*)</a>";

            team[0] = "http://data.caida.org/datasets/topology/ipv4.allpref24-aslinks/team-1/";
            team[1] = "http://data.caida.org/datasets/topology/ipv4.allpref24-aslinks/team-2/";
            team[2] = "http://data.caida.org/datasets/topology/ipv4.allpref24-aslinks/team-3/";

            dati = new ArrayList();

            foreach (string teamx in team)
            {
                Console.WriteLine("Analizzo {0}", teamx);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(teamx);
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string html = reader.ReadToEnd();
                        Regex regex = new Regex(regexp);
                        MatchCollection matches = regex.Matches(html);
                        if (matches.Count > 0)
                        {
                            foreach (Match match in matches)
                            {
                                if (match.Success)
                                {
                                    Console.WriteLine("Daje!");
                                    string anno = match.Groups["anno"].ToString();
                                    string path_directory = teamx + anno;
                                    dati.Add(path_directory);
                                    
                                    string output = Path.Combine(Environment.CurrentDirectory,anno);
                                    if (!System.IO.Directory.Exists(output))
                                        System.IO.Directory.CreateDirectory(output);

                                    fetchFile(path_directory, anno);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void fetchFile(string path, string dir)
        {
            string regexp = "<a href=\"cycle*.*\">(?<name>.*)</a>";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(path);
            
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    Regex regex = new Regex(regexp);
                    MatchCollection matches = regex.Matches(html);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                    string nome_file = match.Groups["name"].ToString();
                                    WebClient file = new WebClient();
                                    string output_file_tmp = Path.Combine(Environment.CurrentDirectory, dir.Substring(0,4));
                                    string mese = nome_file.Substring(nome_file.Length - 11, 2);

                                    string directory_mese = Path.Combine(output_file_tmp, mese);
                                    if (!System.IO.Directory.Exists(directory_mese))
                                        System.IO.Directory.CreateDirectory(directory_mese);

                                    string output_file = Path.Combine(directory_mese, nome_file);
                                    Console.WriteLine("Sto scaricando {0}  in {1}", path + nome_file, output_file);
                                    if (!File.Exists(output_file))
                                    {
                                        file.DownloadFile(path + nome_file, output_file);
                                        Console.WriteLine("Scaricato");
                                        Console.WriteLine("done.");
                                    }
                                    else
                                    {
                                        Console.WriteLine("File {0} gia esistente", output_file);
                                        Console.WriteLine("done.");
                                    }

                                }

                            }

                        }

                    }
                }
        }

    }

    public class Dimes
    {
        public Dimes() : this("2009") {}

        public Dimes(string my_year)
        {
    
            string regexp = "<a href=\"ASEdges.*_" + my_year + "+.csv.gz\">(?<nome>.*ASEdges)(?<mese>[0-9].*)(?<separatore>_)(?<anno>[0-9].*)(?<estensione>.csv.gz)</a>";
            string my_url = "http://www.netdimes.org/PublicData/csv/";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(my_url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    Regex regex = new Regex(regexp);
                    MatchCollection matches = regex.Matches(html);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                string anno = match.Groups["anno"].ToString();
                                string output_tmp = Path.Combine(Environment.CurrentDirectory, anno);

                                string mese = match.Groups["mese"].ToString();

                                if (Int32.Parse(mese) < 10)
                                {
                                    mese = "0" + mese;
                                }

                                string output = Path.Combine(output_tmp,mese);
                                if (!System.IO.Directory.Exists(output_tmp))
                                    System.IO.Directory.CreateDirectory(output_tmp);
                                if (!System.IO.Directory.Exists(output))
                                    System.IO.Directory.CreateDirectory(output);
                                string nome_file = match.Groups["nome"].ToString() + match.Groups["mese"].ToString() + match.Groups["separatore"].ToString() + match.Groups["anno"].ToString() + match.Groups["estensione"].ToString();
                                string path_file = my_url + nome_file;

                                WebClient file = new WebClient();
                                string output_file = Path.Combine(output, nome_file);
                                Console.WriteLine("Sto scaricando {0}  in {1}", my_url + nome_file, output_file);

                                if (!File.Exists(output_file))
                                {
                                    file.DownloadFile(my_url + nome_file, output_file);
                                    Console.WriteLine("Scaricato");
                                }
                                else
                                {
                                    Console.WriteLine("File {0} gia esistente", output_file);
                                }
                            }
                        }
                    }
                }
            }
        }

    }

    public class Irl
    {
        public Irl() : this("2009") {}

        public Irl(string my_year) 
        {
            string indirizzo = "http://irl.cs.ucla.edu/topology/data/";
            string regexp = "<a href=\"[0-9].*\">(?<anno>.+)</a>";

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(indirizzo);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    Regex regex = new Regex(regexp);
                    MatchCollection matches = regex.Matches(html);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                string anno = match.Groups["anno"].ToString().Substring(1, 4);
                                string mese = match.Groups["anno"].ToString().Substring(match.Groups["anno"].ToString().Length - 3, 2);

                                if (anno.Equals(my_year))
                                {
                                    string nuovo_url = Path.Combine(indirizzo, match.Groups["anno"].ToString().Substring(1,match.Groups["anno"].ToString().Length -1 ));
                                    fetchFile(nuovo_url,mese,anno);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void fetchFile(string my_path, string my_mese,string my_anno)
        {
            string output_file_tmp = Path.Combine(Environment.CurrentDirectory,my_anno);
            string output_dir = Path.Combine(output_file_tmp, my_mese);
            string regexp = "<a href=\"links.*\">(?<file>.*)</a>";

            if (!System.IO.Directory.Exists(output_file_tmp))
                System.IO.Directory.CreateDirectory(output_file_tmp);
            if (!System.IO.Directory.Exists(output_dir))
                System.IO.Directory.CreateDirectory(output_dir);

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(my_path);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    string html = reader.ReadToEnd();
                    Regex regex = new Regex(regexp);
                    MatchCollection matches = regex.Matches(html);
                    if (matches.Count > 0)
                    {
                        foreach (Match match in matches)
                        {
                            if (match.Success)
                            {
                                string nome_file = match.Groups["file"].ToString().Substring(1, match.Groups["file"].ToString().Length - 1);
                                string nome_file_completo = Path.Combine(output_dir,nome_file);
                                WebClient file = new WebClient();
                                Console.WriteLine("Sto scaricando {0} in {1}", my_path + nome_file.Substring(0,nome_file.Length ), nome_file_completo);
                                if (!File.Exists(nome_file_completo))
                                {
                                    file.DownloadFile(my_path + nome_file.Substring(0, nome_file.Length), nome_file_completo);
                                }
                                else
                                {
                                    Console.WriteLine("File {0} gia esistente", nome_file_completo);
                                }

                            }
                        }
                    }
                }
            }


        }
    }

    public class Decompressore
    {
        public Decompressore() : this("2009") {}

        public Decompressore(string dir_anno)
        {
            string[] listaDir = Directory.GetDirectories(dir_anno);

            foreach (string mese in listaDir)
            {
                Console.WriteLine(mese);
                string[] m = mese.Split(Path.DirectorySeparatorChar);
                string nome_mese = m[m.Length - 1];

                string dest_path = Path.Combine(Environment.CurrentDirectory, dir_anno);
                dest_path = Path.Combine(dest_path, nome_mese);

                string[] fileCompressi = Directory.GetFiles(mese, "*.gz");

                foreach (string a in fileCompressi)
                {
                    string[] a2 = a.Split(Path.DirectorySeparatorChar);
                    string nome_file = a2[a2.Length-1];
                    if (!File.Exists(Path.Combine(dest_path,nome_file.Substring(0,nome_file.Length-3))))
                    {
                        Console.WriteLine("Scompatto il file: {0}", Path.Combine(dest_path, nome_file.Substring(0, nome_file.Length - 3)));
                        Decompress(dest_path, nome_file);

                        Console.WriteLine("done.");
                    }
                    else
                    {
                        Console.WriteLine("File {0} gia scompattato", Path.Combine(dest_path, nome_file.Substring(0, nome_file.Length - 3)));
                        Console.WriteLine("done.");
                    }
                }


            }
        }

        public  void Decompress(string pathDir, string nome_file)
        {
            string dstFile = "";
            FileStream fsIn = null;
            FileStream fsOut = null;
            GZipStream gzip = null;
            const int bufferSize = 4096;

            byte[] buffer = new byte[bufferSize];

            int count = 0;

            try
            {
                string nome_f = nome_file.Substring(0, nome_file.Length - 3);
                dstFile = Path.Combine(pathDir,nome_f);
                string input_file = Path.Combine(pathDir, nome_file);

                Console.WriteLine("Decomprimo {0} in {1}", input_file, dstFile);

                fsIn = new FileStream(input_file, FileMode.Open, FileAccess.Read, FileShare.Read);
                fsOut = new FileStream(dstFile, FileMode.Create, FileAccess.Write, FileShare.None);
                gzip = new GZipStream(fsIn, CompressionMode.Decompress, true);

                while (true)
                {
                    count = gzip.Read(buffer, 0, bufferSize);
                    if (count != 0)
                    {
                        fsOut.Write(buffer, 0, count);
                    }

                    if (count != bufferSize)
                    {
                        break;
                    }
                }
            }

            catch (Exception ex)
            {
                System.Diagnostics.Debug.Assert(false, ex.ToString());
            }

            finally
            {
                if (gzip != null)
                {
                    gzip.Close();
                    gzip = null;
                }

                if (fsOut != null)
                {
                    fsOut.Close();
                    fsOut = null;
                }

                if (fsIn != null)
                {
                    fsIn.Close();
                    fsIn = null;
                }
            }
        } 
    }

    public class Mappaggio
    {

        string[] AS_internet;
        string[] AS_caida;
        string[] AS_irl;
        string[] AS_dimes;

        Hashtable ASmapping;
        Hashtable ASmapping_legenda;

        public Mappaggio() : this("2009") { }

        public Mappaggio(string my_year)
        {
            ASmapping = new Hashtable();
            ASmapping_legenda = new Hashtable();

            string cartella_AS = Path.Combine(Environment.CurrentDirectory, "output");

            AS_internet = Directory.GetFiles(cartella_AS, "AS_internet*" + my_year + "*_originale.txt");
            AS_caida = Directory.GetFiles(cartella_AS, "AS_caida*" + my_year + "*_originale.txt");
            AS_irl = Directory.GetFiles(cartella_AS, "AS_irl*" + my_year + "*_originale.txt");
            AS_dimes = Directory.GetFiles(cartella_AS, "AS_dimes*" + my_year + "*_originale.txt");

            foreach (string m in AS_internet)
            {
                Console.WriteLine("Elaborazione file {0} in corso",m);
                mappaGrafo(m);
                Console.WriteLine("Elaborazione file {0} completato",m);
            }
            foreach (string m in AS_caida)
            {
                Console.WriteLine("Elaborazione file {0} in corso", m);
                mappaGrafo(m);
                Console.WriteLine("Elaborazione file {0} completato", m);
            }
            foreach (string m in AS_irl)
            {
                Console.WriteLine("Elaborazione file {0} in corso", m);
                mappaGrafo(m);
                Console.WriteLine("Elaborazione file {0} completato", m);
            }
            foreach (string m in AS_dimes)
            {
                Console.WriteLine("Elaborazione file {0} in corso", m);
                mappaGrafo(m);
                Console.WriteLine("Elaborazione file {0} completato", m);
            }

        }

        public void mappaGrafo(string nome_file)
        {
            ASmapping.Clear();
            ASmapping_legenda.Clear();
            int contatore = 0;
            string nome_file_mappato = nome_file.Substring(0, nome_file.Length - 14) + "_mappato.txt";
            string nome_file_legenda = nome_file.Substring(0, nome_file.Length - 14) + "_legenda.txt";

            StreamWriter stream_mappato = new StreamWriter(nome_file_mappato);
            StreamWriter stream_legenda = new StreamWriter(nome_file_legenda);

            try
            {
                using (StreamReader stream_originale = new StreamReader(nome_file))
                {
                    string sLine;
                    string sLine_output;
                    while ((sLine = stream_originale.ReadLine()) != null)
                    {
                        if (sLine != null)
                        {
                            string[] autonomous_system = sLine.Split('\t');

                            if (!ASmapping.ContainsKey(autonomous_system[0]))
                            {
                                ASmapping.Add(autonomous_system[0], contatore);
                                ASmapping_legenda.Add(contatore, autonomous_system[0]);
                                contatore++;
                            }
                            if (!ASmapping.ContainsKey(autonomous_system[1]))
                            {
                                ASmapping.Add(autonomous_system[1], contatore);
                                ASmapping_legenda.Add(contatore, autonomous_system[1]);
                                contatore++;
                            }
                            sLine_output = ASmapping[autonomous_system[0]].ToString() + '\t' + ASmapping[autonomous_system[1]].ToString();
                            stream_mappato.WriteLine(sLine_output); 
                        }
                    } stream_originale.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
            }

            foreach (DictionaryEntry d in ASmapping_legenda)
            {
                stream_legenda.WriteLine(d.Key.ToString() + '\t' + d.Value.ToString());
            }
            
            stream_mappato.Close();
            stream_legenda.Close();
        }
    }
}

