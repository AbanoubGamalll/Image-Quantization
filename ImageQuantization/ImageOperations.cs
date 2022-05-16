using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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



        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //////////////////////////////////////
            //Graph construction
            //////////////////////////////////////

            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            // Checking if the color is visted or not by A new Array 

            int[,,] visted = new int[260, 260, 260];

            //Map passing by every colour to select k-cluster  

            RGBPixel[,,] map = new RGBPixel[260, 260, 260];

            // Array selecting Distinct colours

            RGBPixel[] distinct = new RGBPixel[6000000];

            int ptr = 0;

/*            for (int i = 0; i < 256; i++)
                for (int j = 0; j < 256; j++)
                    for (int k = 0; k < 256; k++)
                        visted[i, j, k] = 0;
*/

            //complexty=N^2
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    RGBPixel r = ImageMatrix[i, j];

                    if (visted[r.red, r.green, r.blue] == 0)
                    {
                        distinct[ptr++] = ImageMatrix[i, j];
                        visted[r.red, r.green, r.blue] = 1;
                    }
                }

            ///////////////////////////////////////////////
            //MST Code impilimentation
            //////////////////////////////////////////////

            ArrayList[] adj = new ArrayList[ptr];
            // to store the colours in the same cluster
            ArrayList[] cluster = new ArrayList[ptr];
          
            for (int i = 0; i < ptr; i++)
            {
                adj[i] = new ArrayList();
                cluster[i] = new ArrayList();
            }
            // visited array to check the node aded for the mst or not
            int[] vis = new int[ptr];
            int n = ptr;
            edje[] mst = new edje[n - 1];
            int indx = 0;
            for (int i = 0; i < n; i++)
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
            // first add the first colour to the mst 
            for (int i = 1; i < n; i++)
            {
                mn[i].w = calc(distinct[i], distinct[0]);
                mn[i].u = i;
                mn[i].v = 0;
            }
            vis[0] = 1;
            mn[0].w = 1e9;
            while (true)
            {
                // every time add one node with the lowest cost for mst
                bool f = false;
                temp.w = 1e9;
                for (int i = 1; i < n; i++)
                {
                    if (temp.w > mn[i].w)
                    {
                        temp = mn[i];
                        f = true;
                    }
                }
                // if i didnt add any node i sure that the mst completed and break
                if (!f)
                    break;
                // update the minmum edje for every node
                mst[indx++] = temp;
                sum += temp.w;
                vis[temp.u] = 1;
                mn[temp.u].w = 1e9;
                for (int i = 1; i < n; i++)
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
            // print the mst sum in the form
            MainForm.v = sum;
            //////////////////////////////////////////////////


            /////////////////////////////////////////
            ///Palette Generation (k-Clusters)
            //////////////////////////////////////////


            ///Sort by Launda expression
            Array.Sort(mst, (x, y) => y.w.CompareTo(x.w));
            // Saving K-clusters
            int kc = (int)sigma;
            // Colours of index  To be used 
            RGBPixel[] K_Colours = new RGBPixel[kc];
            indx = 0;
            // Remove the biggest edges of the colour k to make the MST k clusters 
            for (int i = kc - 1; i < n - 1; i++)
            {
                int u = mst[i].u, v = mst[i].v;
                //Adding each colour From the spaning tree
                adj[u].Add(v);
                adj[v].Add(u);
            }
            //Make a new non-visted array
            for (int i = 0; i < n; i++)
            {
                vis[i] = 0;
            }

            for (int i = 0; i < n; i++)
            {
                //Check if visted
                if (vis[i] == 1)
                    continue;
                //Enqueue Each visted vertex of the tree
                Queue<int> qe = new Queue<int>();
                qe.Enqueue(i);
                //////////
                while (qe.Count > 0)
                {
                    int node = qe.Dequeue();
                    cluster[indx].Add(node);
                    vis[node] = 1;
                    foreach (int j in adj[node])
                    {
                        if (vis[j] == 1)
                            continue;
                        vis[j] = 1;
                        qe.Enqueue(j);
                    }
                }
                indx++;
                ////////////////
            }
            indx = 0;


            for (int i = 0; i < kc; i++)
            {
                // loop for every clusters and select the colour with max distance is minmum
                double w = 1e9;
                int center = 0;
                //checking on each cluster 
                foreach (int j in cluster[i])
                {

                    double cur = 0;
                    //Check to get max distance between each cluster
                    foreach (int k in cluster[i])
                    {
                        cur = Math.Max(cur, calc(distinct[j], distinct[k]));
                    }
                    //Exchange Each MAX Distance Between Each Cluster
                    if (cur < w)
                    {
                        w = cur;
                        center = j;
                    }
                }
                K_Colours[indx++] = distinct[center];
            }

            for (int i = 0; i < n; i++) 
            {
                // chose The Best colour (Minimum Available Colour)
                double w = 1e9;
                RGBPixel col;
                col.red = 0;
                col.green = 0;
                col.blue = 0;
                for (int j = 0; j < kc; j++)
                {
                    if (calc(distinct[i], K_Colours[j]) < w)
                    {
                        w = calc(distinct[i], K_Colours[j]);
                        col = K_Colours[j];
                    }
                }
                //Adding each distinct colour at the map
                map[distinct[i].red, distinct[i].green, distinct[i].blue] = col;
            }
            // print the final Cluster Colour
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    int r = ImageMatrix[i, j].red;
                    int g = ImageMatrix[i, j].green;
                    int b = ImageMatrix[i, j].blue;
                    ImageMatrix[i, j] = map[r, g, b];

                }

            stopwatch.Stop();
            MainForm.show_time(stopwatch.ElapsedMilliseconds / 1000.00);
            

            return ImageMatrix;
            ///////////////////////////////////////
        }
      


    }
}
