using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using openalprnet;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Math;
using System.Data;
using System.Data.SqlClient;




namespace PlakaTanima
{
    public partial class AnaForm : Form
    {
        public AnaForm()
        {
            InitializeComponent();
        }
        SqlConnection baglantı = new SqlConnection(@"Data Source=LAPTOP-VKE3OR56\SQL17;Initial Catalog=ıdeaotopark;Integrated Security=True");
        FilterInfoCollection fico;
        VideoCaptureDevice vcd;

        public static string AssemblyDirectory
        {
            get
            {
                var codeBase = Assembly.GetExecutingAssembly().CodeBase;
                var uri = new UriBuilder(codeBase);
                var path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }

        public Rectangle boundingRectangle(List<Point> points)
        {
           

            var minX = points.Min(p => p.X);
            var minY = points.Min(p => p.Y);
            var maxX = points.Max(p => p.X);
            var maxY = points.Max(p => p.Y);

            return new Rectangle(new Point(minX, minY), new Size(maxX - minX, maxY - minY));
        }

        private static Image cropImage(Image img, Rectangle cropArea)
        {
            var bmpImage = new Bitmap(img);
            return bmpImage.Clone(cropArea, bmpImage.PixelFormat);
        }

        public static Bitmap combineImages(List<Image> images)
        {
            Bitmap finalImage = null;

            try
            {
                var width = 0;
                var height = 0;

                foreach (var bmp in images)
                {
                    width += bmp.Width;
                    height = bmp.Height > height ? bmp.Height : height;
                }
                finalImage = new Bitmap(width, height);
                using (var g = Graphics.FromImage(finalImage))
                {
                    g.Clear(Color.Black);
                    var offset = 0;
                    foreach (Bitmap image in images)
                    {
                        g.DrawImage(image,
                                    new Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }

                return finalImage;
            }
            catch (Exception ex)
            {
                if (finalImage != null)
                    finalImage.Dispose();

                throw ex;
            }
            finally
            {
                foreach (var image in images)
                {
                    image.Dispose();
                }
            }
        }

        private void processImageFile(string fileName)
        {
            resetControls();
            var region = checkAmerika.Checked ? "us" : "eu";
            String config_file = Path.Combine(AssemblyDirectory, "openalpr.conf");
           
            String runtime_data_dir = Path.Combine(AssemblyDirectory, "runtime_data");
           
            using (var alpr = new AlprNet(region, config_file, runtime_data_dir))
            {
                if (!alpr.IsLoaded())
                {
                    txtPlaka.Text="Kütüphane Yüklenemedi";
                    return;
                }
                picOriginResim.ImageLocation = fileName;
                picOriginResim.Load();

                var results = alpr.Recognize(fileName);

                var images = new List<Image>(results.Plates.Count());
                var i = 1;
                foreach (var result in results.Plates)
                {
                    var rect = boundingRectangle(result.PlatePoints);
                    var img = Image.FromFile(fileName);
                    var cropped = cropImage(img, rect);
                    images.Add(cropped);

                    txtPlaka.Text = EnBenzeyenPlakayiGetir(result.TopNPlates);
                }

                if (images.Any())
                {
                    picPlakaResmi.Image = combineImages(images);
                }
            }
        }

        private string EnBenzeyenPlakayiGetir(List<AlprPlateNet> plakalar)
        {
            foreach (var item in plakalar)
            {
                string okunan = item.Characters.PadRight(50);
                string karakter = okunan.Trim();
                var chars = karakter.ToCharArray();
                int k = chars.Length;

                for (int i = 0; i <=k; i++)
                {
                    if (chars[0] == 'O')
                    {
                        chars[0] = '0';
                    }
                    if (chars[1] == 'O')
                    {
                        chars[1] = '0';
                    }
                    if (chars[5] == 'O')
                    {
                        chars[5] = '0';
                    }
                    if (chars[6] == 'O')
                    {
                        chars[6] = '0';
                    }
                   
                    string dönen = new string(chars);
                    return dönen;


                }
               
              
                        return karakter;
                    }
                

            
            return "";
        }

        private void resetControls()
        {
            picOriginResim.Image = null;
            picPlakaResmi.Image = null;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            resetControls();
           

        }

        private void btnPlakayiBul_Click(object sender, EventArgs e)
        {
            txtPlaka.Text = "";
            picPlakaResmi.Image= null;
            üyetxt.Text = "";
            ücrettxt.Text = "";
            süretxt.Text = "";
            giriszamanxt.Text = "";
            cıkıszamantxt.Text = "";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                processImageFile(openFileDialog.FileName);
            }
        }

        private void buttoncambasla_Click(object sender, EventArgs e)
        {
            vcd = new VideoCaptureDevice(fico[comboBox1.SelectedIndex].MonikerString);
            vcd.NewFrame += Vcd_NewFrame;
            vcd.Start();
            timer1.Start();
        }
        private void Vcd_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            picturecam.Image = (Bitmap)eventArgs.Frame.Clone();
        }

        private void buttoncamyakala_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "(*.jpg)|*.jpg";
            DialogResult dr = sfd.ShowDialog();
            if (dr == DialogResult.OK)
            {
                picturecam.Image.Save(sfd.FileName);
            }
            timer1.Stop();
        }

        private void buttoncamsec_Click(object sender, EventArgs e)
        {

            fico = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            foreach (FilterInfo f in fico)
            {
                comboBox1.Items.Add(f.Name);
                comboBox1.SelectedIndex = 0;
            }
        }

        private void buttoncamkapa_Click(object sender, EventArgs e)
        {
            vcd.Stop();
            picturecam.Image = null;
        }

        private void ÜyeKayıtBut_Click(object sender, EventArgs e)
        {
            ÜyeKayıt kayıt = new ÜyeKayıt();
            kayıt.ShowDialog();
        }

        private void aracgirisbut_Click(object sender, EventArgs e)
        {
            string plaka = txtPlaka.Text;
            baglantı.Open();
           

            SqlCommand komut = new SqlCommand("select *from üyeler  ", baglantı);
          
            komut.ExecuteNonQuery();

            SqlDataReader oku = komut.ExecuteReader();
          
           
            

            while (oku.Read())
            {
                string okuplaka = oku["plaka"].ToString();
               
               
                if (okuplaka==plaka)
                {
                    üyetxt.Text = "ÜYE";

                    break;
                }
                else
                {
                   
                    üyetxt.Text = "ÜYE DEĞİL";
                   
                    
                 
                   

                }
                

            }
            oku.Close();
         

            baglantı.Close();
            if (üyetxt.Text == "ÜYE DEĞİL")
            {
                üyeolmayanlar();
            }
        }
       

        private void üyeolmayanlar()
        {
            baglantı.Open();
            
            SqlCommand ücretli = new SqlCommand("insert into ücretli(plaka,giriszamanı,cıkıszamanı) values(@plaka,@giriszamanı,@cıkıszamanı)", baglantı);
           
            ücretli.Parameters.AddWithValue("@plaka", txtPlaka.Text);
            ücretli.Parameters.AddWithValue("@giriszamanı", DateTime.Now.ToString());
            ücretli.Parameters.AddWithValue("@cıkıszamanı", " ");
            ücretli.ExecuteNonQuery();





            baglantı.Close();
        }

        private void araccıkısbut_Click(object sender, EventArgs e)
        {
            string plaka = txtPlaka.Text;
            baglantı.Open();


            SqlCommand komut = new SqlCommand("select *from üyeler  ", baglantı);

            komut.ExecuteNonQuery();

            SqlDataReader oku = komut.ExecuteReader();




            while (oku.Read())
            {
                string okuplaka = oku["plaka"].ToString();


                if (okuplaka == plaka)
                {
                    üyetxt.Text = "ÜYE";
                    ücrettxt.Text = "0 TL";

                    break;
                }
                else
                {

                    üyetxt.Text = "ÜYE DEĞİL";
                }
            }
            oku.Close();

            baglantı.Close();
            if (üyetxt.Text == "ÜYE DEĞİL")
            {
                cıkıszamanıgüncelle();
            }
        }

        private void cıkıszamanıgüncelle()
        {
            baglantı.Open();

          SqlCommand cıkısguncel = new SqlCommand("update ücretli set cıkıszamanı='"+DateTime.Now.ToString()+"'where plaka='"+txtPlaka.Text+"'", baglantı);
            cıkısguncel.ExecuteNonQuery();
            baglantı.Close();
            hesapla();


        }

        private void hesapla()
        {
            baglantı.Open();
            SqlCommand vericekgiris = new SqlCommand("select giriszamanı from ücretli where plaka='" + txtPlaka.Text + "'",baglantı);
            vericekgiris.ExecuteNonQuery();
            SqlDataReader giriszam = vericekgiris.ExecuteReader();
            while (giriszam.Read())
            { 
                giriszamanxt.Text = giriszam["giriszamanı"].ToString();

            }
            giriszam.Close();
            SqlCommand vericekcıkıs = new SqlCommand("select cıkıszamanı from ücretli where plaka='" + txtPlaka.Text + "'", baglantı);
            vericekcıkıs.ExecuteNonQuery();
            SqlDataReader cıkıszam = vericekcıkıs.ExecuteReader();
            while (cıkıszam.Read())
            {
                cıkıszamantxt.Text = cıkıszam["cıkıszamanı"].ToString();

            }
            cıkıszam.Close();
            DateTime gelis, cıkıs;
            TimeSpan süre;
            gelis = DateTime.Parse(giriszamanxt.Text);
            cıkıs = DateTime.Parse(cıkıszamantxt.Text);
            süre = cıkıs - gelis;

            string yazan= süre.TotalHours.ToString("0.00");
            süretxt.Text = yazan + " Saat";
            double zaman = Convert.ToDouble(yazan);
            if (zaman < 1)
            {
                ücrettxt.Text = "10.00" + " TL";
            }
            else if((zaman>=1)&(zaman<2))
            {

                ücrettxt.Text = "12.00" + " TL";
            }
            else if ((zaman >= 2) & (zaman < 4))
            {

                ücrettxt.Text = "14.00" + " TL";
            }
            else if ((zaman >= 4) & (zaman < 8))
            {

                ücrettxt.Text = "17.00" + " TL";
            }
            else
            {
                ücrettxt.Text = "20.00" + " TL";
            }





            baglantı.Close();
        }

        private void sistemkapabut_Click(object sender, EventArgs e)
        {
            this.Close();
        }

      
    }
    }
