using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    /// 

    public struct RGBPixel
    {
        public byte red, green, blue;
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }
    public struct edje
    {
        public double w;
        public int u, v;
    }
   

    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    public class ImageOperations
    {
        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static double calc(RGBPixel x, RGBPixel y)
        {
            double ret =  Math.Abs(x.red - y.red)* Math.Abs(x.red - y.red);
            ret += Math.Abs(x.green - y.green) * Math.Abs(x.green - y.green);
            ret += Math.Abs(x.blue - y.blue) * Math.Abs(x.blue - y.blue);
            return Math.Sqrt(ret);
        }

        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }
        
        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);
            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }


        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        /// 
       public static HashSet<RGBPixel> st = new HashSet<RGBPixel>();


        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {

            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];
            int[,,] visted = new int[260, 260, 260];
            RGBPixel []distinct = new RGBPixel[60000];
            int ptr = 0;

            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 256; j++)
                    for (int k = 0; k < 256; k++)
                        visted[i,j,k] = 0;
            //map for get distincit colors              

            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    RGBPixel r = ImageMatrix[i, j];

                    if (visted[r.red,r.green,r.blue]==0)
                    {
                        distinct[ptr++] = ImageMatrix[i, j];
                        visted[r.red, r.green, r.blue] = 1;

                    }
                   
                }            
            ArrayList[] adj = new ArrayList[ptr];
            for(int i = 0;i<ptr;i++)
                adj[i] = new ArrayList();   
            int []vis = new int[ptr];
            int n = ptr;
            for (int i = 0; i <n; i++)
            {
                vis[i] = 0;
            }
            vis[0] = 1;
            double sum = 0;
            edje[] mn = new edje[n];
            edje temp;
            temp.w = 1e9;
            temp.v = 0;
            temp.u = 0;
            for(int i = 1; i < n; i++)
            {
                mn[i].w = calc(distinct[i], distinct[0]);
                mn[i].u = i;
                mn[i].v = 0;
            }
            vis[0] = 1;
            mn[0].w = 1e9;
            while(true)
            {
                bool f = false;
                temp.w = 1e9;
                for(int i =1; i<n; i++)
                {
                    if (temp.w > mn[i].w)
                    {
                        temp = mn[i];
                        f = true;
                    }
                }
                if (!f)
                    break;
                adj[temp.u].Add(temp.v);
                adj[temp.v].Add(temp.u);
                sum += temp.w;
                vis[temp.u] = 1;
                mn[temp.u].w = 1e9;
                for(int i = 1;i<n;i++)
                {
                    if (vis[i] == 1)
                        continue;
                    edje temp2;
                    temp2.w = calc(distinct[temp.u], distinct[i]);
                    temp2.u = i;
                    temp2.v = temp.u;
                    if (mn[i].w > temp2.w)
                        mn[i] = temp2;

                }
            }
            int ret = 0;
            for (int i = 0; i < n; i++)
                ret += vis[i];
            MainForm.v = sum ;

            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            

            //Compute Filter in Spatial Domain :
            //==================================
          

            //Filter Original Image Vertically:
            //=================================
          

            return Filtered;
        }


    }
}
