using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;

namespace Bookmarks
{

    public class oDirectory
    {

        public string Id;
        public string Title;

        public oDirectory(string id, string title)
        {
            Id = id;
            Title = title;
        }

    }

    public class oFile
    {

        public string Title;
        public string Url;

        public oFile(string title, string url)
        {
            Title = title;
            Url = url;
        }

    }

    class Program
    {

        static void Main(string[] args)
        {

            IEnumerable<string> Profiles = Directory.EnumerateDirectories(@"C:\Users\Alain\AppData\Roaming\Mozilla\Firefox\Profiles\");

            using (SQLiteConnection SLCon = new SQLiteConnection(@"Data Source=" + Profiles.First() + @"\places.sqlite;"))
            {

                SLCon.Open();

                using (SQLiteCommand SQLCom = new SQLiteCommand(SLCon))
                {

                    using (FileStream FS = new FileStream("BOOKMARKS.HTM", FileMode.Create))
                    {

                        using (StreamWriter SW = new StreamWriter(FS, Encoding.GetEncoding(1252)))
                        {

                            SW.Write("<html>");
                            SW.Write("<body style='background:#FFCC99;font-family:Arial,Verdana,Helvetica;text-align:justify'>");
                            SW.Write("<h1 style='background:#99CCFF;border:medium outset;color:#0000FF;text-align:center'>BOOKMARKS</h1>");

                            TDirectory(SQLCom, SW, "", "2");

                            SW.Write("<p align='right'>&copy;&nbsp;<a href='http://guimberteau.net/' target='_blank'>Alain Guimberteau</a></p>");
                            SW.Write("</body>");
                            SW.Write("</html>");

                        }

                    }

                }

            }

        }

        static List<oDirectory> GetDirectories(SQLiteCommand SQLCom, StreamWriter SW, string ParentName, string ParentId)
        {

            List<oDirectory> OD = new List<oDirectory>();

            SQLCom.CommandText = "select id,title from moz_bookmarks where type=2 and parent=" + ParentId + " order by position";

            using (SQLiteDataReader SQLDR = SQLCom.ExecuteReader())
            {

                while (SQLDR.Read())
                {

                    NameValueCollection nvc = SQLDR.GetValues();
                    OD.Add(new oDirectory(nvc["id"], ParentName + @"\" + nvc["title"]));

                }

            }

            return OD;

        }

        static void TDirectory(SQLiteCommand SQLCom, StreamWriter SW, string ParentName, string ParentId)
        {

            if (ParentName != "")
            {
                SW.Write("<h2 style='background:#99CCFF;border:thin outset;width:100%'>&nbsp;");
                SW.Write(ParentName.Substring(1).Replace(@"\", "&nbsp;-&nbsp;"));
                SW.Write("&nbsp;</h2>");
            }

            SW.Write("<ul>");

            foreach (var File in GetFiles(SQLCom, SW, ParentId))
            {
                TFile(SW, File);
            }

            SW.Write("</ul>");

            foreach (var Directory in GetDirectories(SQLCom, SW, ParentName, ParentId))
            {
                TDirectory(SQLCom, SW, Directory.Title, Directory.Id);
            }

        }

        static List<oFile> GetFiles(SQLiteCommand SQLCom, StreamWriter SW, string ParentId)
        {

            List<oFile> OF = new List<oFile>();

            SQLCom.CommandText = "select b.title as title,url from moz_bookmarks as b join moz_places as p on b.fk=p.id where type=1 and parent=" + ParentId + "  order by position";

            using (SQLiteDataReader SQLDR = SQLCom.ExecuteReader())
            {

                while (SQLDR.Read())
                {

                    NameValueCollection nvc = SQLDR.GetValues();
                    OF.Add(new oFile(nvc["title"], nvc["url"]));

                }

            }

            return OF;

        }

        static void TFile(StreamWriter SW, oFile File)
        {

            SW.Write("<li><a href='");
            SW.Write(File.Url);
            SW.Write("'>");
            SW.Write(File.Title);
            SW.Write("</a></li>");

        }

    }

}