using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace PlakaTanima
{
    public partial class ÜyeKayıt : Form
    {
        public ÜyeKayıt()
        {
            InitializeComponent();
        }
        SqlConnection baglantı = new SqlConnection(@"Data Source=LAPTOP-VKE3OR56\SQL17;Initial Catalog=ıdeaotopark;Integrated Security=True");
        private void ÜyeKayıt_Load(object sender, EventArgs e)
        {
            

        }

        private void çıkışbut_Click(object sender, EventArgs e)
        {

            this.Close();
        }

        private void kaydetbut_Click(object sender, EventArgs e)
        {
            baglantı.Open();
            SqlCommand komut = new SqlCommand("insert into üyeler(plaka,ad,soyad,marka,model,telefon,email) values(@plaka,@ad,@soyad,@marka,@model,@telefon,@email)", baglantı);
            komut.Parameters.AddWithValue("@plaka", plakatxt.Text);
            komut.Parameters.AddWithValue("@ad", adtxt.Text);
            komut.Parameters.AddWithValue("@soyad", soyadtxt.Text);
            komut.Parameters.AddWithValue("@marka", markatxt.Text);
            komut.Parameters.AddWithValue("@model", modeltxt.Text);
            komut.Parameters.AddWithValue("@telefon", teltxt.Text);
            komut.Parameters.AddWithValue("@email", emailtxt.Text);
            komut.ExecuteNonQuery();
            baglantı.Close();
            MessageBox.Show("Araç Kaydı Oluşturuldu", "Kayıt");
            foreach (Control item in üyegroupBox.Controls)
            {
                if (item is TextBox)
                {
                    item.Text = "";
                }
            }
            baglantı.Close();
        }

        private void üyelerigösterbut_Click(object sender, EventArgs e)
        {
            üyelerigetir();

        }

        private void üyelerigetir()
        {
           
            listView1.Items.Clear();
            listView1.Columns.Add("Plaka");
            listView1.Columns.Add("Ad");
            listView1.Columns.Add("Soyad");
            listView1.Columns.Add("Marka");
            listView1.Columns.Add("Model");
            listView1.Columns.Add("Telefon");
            listView1.Columns.Add("Email");
            listView1.View = View.Details;
            listView1.GridLines = true;
            

            baglantı.Open();
            SqlCommand üyelergetir = new SqlCommand("select *from üyeler", baglantı);
            SqlDataReader üyeler = üyelergetir.ExecuteReader();
            while (üyeler.Read())
            {
                ListViewItem item = new ListViewItem(üyeler["plaka"].ToString());
                item.SubItems.Add(üyeler["ad"].ToString());
                item.SubItems.Add(üyeler["soyad"].ToString());
                item.SubItems.Add(üyeler["marka"].ToString());
                item.SubItems.Add(üyeler["model"].ToString());
                item.SubItems.Add(üyeler["telefon"].ToString());
                item.SubItems.Add(üyeler["email"].ToString());
                listView1.Items.Add(item);
                listView1.FullRowSelect = true;


            }

            baglantı.Close();
        }


        string plaka = null;
        private void Üyesilbut_Click(object sender, EventArgs e)
        {
            baglantı.Open();
            SqlCommand sil = new SqlCommand("delete from üyeler where plaka='" + plaka +"'", baglantı);
            sil.ExecuteNonQuery();
            baglantı.Close();
            üyelerigetir();
            silplakatxt.Clear();
            siladtxt.Clear();
            silsoyadtxt.Clear();
            silmarkatxt.Clear();
            silmodeltxt.Clear();
            siltelefontxt.Clear();
            silemailtxt.Clear();
            MessageBox.Show("Araç Kaydı Silindi ", "Silme");

        }

        private void listView1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            plaka = listView1.SelectedItems[0].SubItems[0].Text;
            silplakatxt.Text = listView1.SelectedItems[0].SubItems[0].Text;
            siladtxt.Text = listView1.SelectedItems[0].SubItems[1].Text;
            silsoyadtxt.Text = listView1.SelectedItems[0].SubItems[2].Text;
            silmarkatxt.Text = listView1.SelectedItems[0].SubItems[3].Text;
            silmodeltxt.Text = listView1.SelectedItems[0].SubItems[4].Text;
            siltelefontxt.Text = listView1.SelectedItems[0].SubItems[5].Text;
            silemailtxt.Text = listView1.SelectedItems[0].SubItems[6].Text;
        }
    }
}
