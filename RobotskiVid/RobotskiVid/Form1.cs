using Emgu.CV;
using Emgu.CV.Structure;
using Emgu.CV.UI;
using System;
using System.Drawing;

using System.Windows.Forms;

namespace RobotskiVid
{
    public partial class Form1 : Form
    {
        Rectangle rect;
        Rectangle rect_letter;
        VideoCapture cap = new VideoCapture(0);
        bool cropFlag = false;
        bool templateFlag = false;
        bool draw = false;
        
        public Form1()
        {
            InitializeComponent();
        }

        private void browse_Click(object sender, EventArgs e)
        {
            // open file dialog   
            OpenFileDialog open = new OpenFileDialog();
            // image filters  
            open.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
            if (open.ShowDialog() == DialogResult.OK)
            {
                // display image in picture box  
                
                pictureBox1.Image = new Bitmap(open.FileName);
                // image file path  
               // textBox1.Text = open.FileName;
            }

        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (cropFlag || templateFlag)
            {
                rect = new System.Drawing.Rectangle(e.X, e.Y, 0, 0);
                this.pictureBox1.Invalidate();

            }
            
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (cropFlag || templateFlag)
            {
                if (e.Button == MouseButtons.Left)
                {

                    rect = new System.Drawing.Rectangle(rect.Left, rect.Top, e.X - rect.Left, e.Y - rect.Top);
                }
                this.pictureBox1.Invalidate();
            }
          
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)

        {

            if (cropFlag || templateFlag)
            {
              
                
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Red, 2);
                e.Graphics.DrawRectangle(pen, rect);
            }

            if (draw)
            {
                System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Green, 2);
                e.Graphics.DrawRectangle(pen, rect_letter);
            }
          
           
        }

        private void CropButton_Click(object sender, EventArgs e)
        {
            cropFlag = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (cropFlag)
            {
                // Create a Bitmap object from a file.
                Bitmap cropedBitmap = new Bitmap(pictureBox1.Image);

                // Clone a portion of the Bitmap object.
                //Rectangle cloneRect = new Rectangle(0, 0, 100, 100);
                System.Drawing.Imaging.PixelFormat format =
                    cropedBitmap.PixelFormat;
                Bitmap cloneBitmap = cropedBitmap.Clone(rect, format);

                ImageViewer viewer = new ImageViewer();
                Image<Bgr, Byte> cvimage = new Image<Bgr, Byte>(cloneBitmap);

                viewer.Image = cvimage;
                viewer.ShowDialog();
                // pictureBox2.Image = cropedBitmap;
                // Draw the cloned portion of the Bitmap object.
                // e.Graphics.DrawImage(cloneBitmap, 0, 0);


                
       

                //CANNY DETEKTOR

                CvInvoke.UseOpenCL = true;
               
                //Bitmap bm = new Bitmap(pictureBox1.Image);
                Bitmap bm = new Bitmap(cvimage.Bitmap);


                Image<Gray, byte> im = new Image<Gray, byte>(bm);
               // cvimage.ToBitmap();
               // UMat u = cvimage.ToUMat();
                UMat u = im.ToUMat();

                CvInvoke.Canny(u, u, 150, 50);

                //pictureBox1.Image = u.Bitmap;


                //convert to image
                //Image<Bgr, Byte> cvimage = new Image<Bgr, Byte>(u.Bitmap);
                Image<Bgr, byte> outImage = new Image<Bgr, byte>(u.Bitmap);

                viewer.Image = outImage;
                viewer.ShowDialog();



                cropFlag = false;
            }



            //LV2 template matching

            if(templateFlag )
            {
                // Create a Bitmap object from a file.
                Bitmap cropedBitmap = new Bitmap(pictureBox1.Image);

                // Clone a portion of the Bitmap object.
                //Rectangle cloneRect = new Rectangle(0, 0, 100, 100);
                System.Drawing.Imaging.PixelFormat format =
                    cropedBitmap.PixelFormat;
                Bitmap cloneBitmap = cropedBitmap.Clone(rect, format);

                ImageViewer viewer = new ImageViewer();
                Image<Bgr, Byte> templ = new Image<Bgr, Byte>(cloneBitmap);

                viewer.Image = templ;
                viewer.ShowDialog();


                Bitmap image_test = new Bitmap(pictureBox1.Image);
                Image<Bgr, Byte> myImage = new Image<Bgr, Byte>(image_test);
                Image<Gray, float> res;

                //template matching metode
                string selectedItem = methodBox.Items[methodBox.SelectedIndex].ToString();

                if (selectedItem == "SQDIFF")
                {
                     res = myImage.MatchTemplate(templ, Emgu.CV.CvEnum.TemplateMatchingType.Sqdiff);
                }else if(selectedItem == "SQDIFF_NORMED")
                {
                     res = myImage.MatchTemplate(templ, Emgu.CV.CvEnum.TemplateMatchingType.SqdiffNormed);

                }
                else if (selectedItem == "CCORR")
                {
                     res = myImage.MatchTemplate(templ, Emgu.CV.CvEnum.TemplateMatchingType.Ccorr);

                }
                else if (selectedItem == "CCORR_NORMED")
                {
                     res = myImage.MatchTemplate(templ, Emgu.CV.CvEnum.TemplateMatchingType.CcorrNormed);

                }
                else if (selectedItem == "CCOEFF")
                {
                     res = myImage.MatchTemplate(templ, Emgu.CV.CvEnum.TemplateMatchingType.Ccoeff);

                }
                else if (selectedItem == "CCOEFF_NORMED")
                {
                     res = myImage.MatchTemplate(templ, Emgu.CV.CvEnum.TemplateMatchingType.CcoeffNormed);

                }
                else { res = null; }
                
           

                // Image<Gray, float> res = myImage.MatchTemplate(templ, Emgu.CV.CvEnum.TemplateMatchingType.);
                viewer.Image = res;
                //viewer.ShowDialog();


                //cols and rows 
                int icols = myImage.Cols - templ.Cols + 1;
                int irows = myImage.Rows - templ.Rows + 1;

                //treshold
                double thresh =(double)Convert.ToDouble(threshBox.Text);
               // MessageBox.Show(thresh.ToString());

                int windowSize = 11;
                int windowRad = (int)(windowSize / 2);
                float centerPixelValue, currentPixelValue;
                bool bcenterPixelMax = false;


                //double ress = res.Norm;
                //prolaz kroz sve pixele

                for ( int y =0; y<irows; y++)
                {
                    for (int x = 0; x < icols; x++)
                    {
                        if ((x - windowRad >= 0) && (x + windowRad <= icols - 1) && (y - windowRad >= 0) && (y + windowRad <= irows - 1))
                        {
                            //vrijednost pixela
                            centerPixelValue = res.Data[y, x, 0];
                            // MessageBox.Show(centerPixelValue.ToString());
                            //MessageBox.Show(centerPixelValue.ToString());
                            if(centerPixelValue > thresh)
                            {
                                //veci od thresholda
                                bcenterPixelMax = true;

                                for(int j =y-windowRad; j<= y+windowRad; j++)
                                {
                                    for (int i = x - windowRad; i <= x + windowRad; i++)
                                    {
                                        currentPixelValue = res.Data[j, i, 0];

                                        //ako je trenutni veci, nije slovo
                                        if (currentPixelValue > centerPixelValue)
                                        {
                                            bcenterPixelMax = false;
                                            break; 
                                        }
                                    }
                                    if (!bcenterPixelMax)
                                        break;  //exit j
                                }

                                //nacrtaj pravokutnik oko slova
                                if (bcenterPixelMax)
                                {
                                   // MessageBox.Show("ima podudaranje!");
                                    draw = true;
                                    rect_letter = new System.Drawing.Rectangle(x, y, templ.Width, templ.Height);
                                     //this.pictureBox1.Invalidate();

                                    System.Drawing.Pen pen = new System.Drawing.Pen(System.Drawing.Color.Green, 2);
                                    //  e.Graphics.DrawRectangle(pen, rect_letter);
                                    Bgr color = new Bgr(0, 255, 0);
                                    myImage.Draw(rect_letter, color, 2);

                                }
                            }

                        }
                    }
                }

                templateFlag = false;
                draw = false;
                viewer.Image = myImage;
                 viewer.ShowDialog();
            }





        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void CaptureButton_Click(object sender, EventArgs e)
        {
            //uslikaj i prikazi sliku
            Image<Bgr, double> capturedImage = cap.QueryFrame().ToImage<Bgr, double>();
            ImageViewer viewer = new ImageViewer();
            cap.Stop();
            pictureBox1.Image = capturedImage.ToBitmap();
            //viewer.Image = capturedImage;
            // viewer.ShowDialog();




            //Bitmap capturedImage_bm = new Bitmap(capturedImage);
            //  Bitmap myImage = new Bitmap(pictureBox1.Image);
            // Image<Bgr, byte> outImage = new Image<Bgr, byte>(myImage);




            // CvInvoke.Canny(capturedImage,outImage, 100,100);










        }

        private void TemplateButton_Click(object sender, EventArgs e)
        {
            

            if(methodBox.SelectedIndex == -1)
            {
                MessageBox.Show("Odaberi metodu!");
            }
            else
            {
                double outD;
                if(double.TryParse(threshBox.Text.ToString(), out outD) ==false)
                {
                    MessageBox.Show("Ne ispravan broj!");
                }
                else
                {
                    string selectedItem = methodBox.Items[methodBox.SelectedIndex].ToString();
                    // label2.Text = selectedItem;
                    templateFlag = true;
                }


               
            }
            

        }
    }
}
