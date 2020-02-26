﻿using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace IKAA_171rdb115_2
{
    public class imgData
    {
        public PixelClassRGB[,] img;
        public PixelClassHSV[,] imghsv;
        public PixelClassCMYK[,] imgcmyk;
        public PixelClassYUV[,] imgyuv;

        ~imgData()
        {
            img = null;
            imghsv = null;
            imgcmyk = null;
            imgyuv = null;
        }
        public void readImage(Bitmap bmp)
        {
            var watchread = System.Diagnostics.Stopwatch.StartNew();
            img = new PixelClassRGB[bmp.Width, bmp.Height];
            imghsv = new PixelClassHSV[bmp.Width, bmp.Height];
            imgcmyk = new PixelClassCMYK[bmp.Width, bmp.Height];
            imgyuv = new PixelClassYUV[bmp.Width, bmp.Height];
            //nolasām datus no attēla
            var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, bmp.PixelFormat);
            //nolasām atmiņā datus par attēlu
            IntPtr ptr = IntPtr.Zero; //mēģinām nolasīt rindu
            int pixelComponents; //kanālu skaits
            if (bmpData.PixelFormat == PixelFormat.Format24bppRgb) //ja ir 24 bitu formāts
            {
                pixelComponents = 3; //kanālu skaits
            }
            else if (bmpData.PixelFormat == PixelFormat.Format32bppRgb) //ja ir 32 bitu formāts
            {
                pixelComponents = 4;
            }
            else pixelComponents = 0;
            var line = new byte[bmp.Width * pixelComponents]; //the length of row array we scan from image
            for (int y = 0; y < bmpData.Height; y++)
            {
                ptr = bmpData.Scan0 + y * bmpData.Stride;
                //nolasām no pirmā pixeļa un stride-pixeļu rinas platums
                Marshal.Copy(ptr, line, 0, line.Length);
                for (int x = 0; x < bmpData.Width; x++)
                {
                    img[x, y] = new PixelClassRGB(line[pixelComponents * x + 2], line[pixelComponents * x + 1], line[pixelComponents * x]); //BGR
                    imghsv[x, y] = new PixelClassHSV(img[x, y].R, img[x, y].G, img[x, y].B);
                    imgcmyk[x, y] = new PixelClassCMYK(img[x, y].R, img[x, y].G, img[x, y].B);
                    imgyuv[x, y] = new PixelClassYUV(img[x, y].R, img[x, y].G, img[x, y].B);
                }
            }
            bmp.UnlockBits(bmpData); //nolasīšanas rezultāts
            watchread.Stop();
            var elapsedMs = watchread.ElapsedMilliseconds;
            Console.WriteLine("Image Read time: " + elapsedMs);
        }

        public Bitmap drawImage(string mode)
        {
            var watchdraw = System.Diagnostics.Stopwatch.StartNew();
            if (img != null)
            {
                IntPtr ptr = IntPtr.Zero;
                int Height = img.GetLength(1);
                int Width = img.GetLength(0);
                var bmp = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
                var bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                var line = new byte[bmp.Width * 3]; //3 kanāli
                for (int y = 0; y < bmpData.Height; y++)
                {
                    for (int x = 0; x < bmpData.Width; x++)
                    {
                        switch (mode)
                        {
                            case "RGB":
                                {
                                    line[3 * x] = img[x, y].B; //blue
                                    line[3 * x + 1] = img[x, y].G; //green
                                    line[3 * x + 2] = img[x, y].R; //red
                                    break;
                                } //rgb
                            case "R":
                                {
                                    line[3 * x] = 0; //blue
                                    line[3 * x + 1] = 0; //green
                                    line[3 * x + 2] = img[x, y].R; //red
                                    break;
                                } //red
                            case "G":
                                {
                                    line[3 * x] = 0; //blue
                                    line[3 * x + 1] = img[x, y].G; //green
                                    line[3 * x + 2] = 0; //red
                                    break;
                                } //green
                            case "B":
                                {
                                    line[3 * x] = img[x, y].B; //blue
                                    line[3 * x + 1] = 0; //green
                                    line[3 * x + 2] = 0; //red
                                    break;
                                } //blue
                            case "I":
                                {
                                    line[3 * x] = img[x, y].I; //blue
                                    line[3 * x + 1] = img[x, y].I; //green
                                    line[3 * x + 2] = img[x, y].I; //red
                                    break;
                                } //grayscale
                            case "Invert":
                                {
                                    line[3 * x] = Convert.ToByte(255 - img[x, y].B); //blue
                                    line[3 * x + 1] = Convert.ToByte(255 - img[x, y].G); //green
                                    line[3 * x + 2] = Convert.ToByte(255 - img[x, y].R); //red
                                    break;
                                } //inverted
                            case "HSV":
                                {
                                    line[3 * x] = img[x, y].hsvToRGB(imghsv[x, y].H, imghsv[x, y].S, imghsv[x, y].V).B; //blue
                                    line[3 * x + 1] = img[x, y].hsvToRGB(imghsv[x, y].H, imghsv[x, y].S, imghsv[x, y].V).G; //green
                                    line[3 * x + 2] = img[x, y].hsvToRGB(imghsv[x, y].H, imghsv[x, y].S, imghsv[x, y].V).R; //red
                                    break;
                                } //hue saturation value
                            case "H":
                                {
                                    line[3 * x] = img[x, y].hsvToRGB(imghsv[x, y].H, 255, 255).B; //blue
                                    line[3 * x + 1] = img[x, y].hsvToRGB(imghsv[x, y].H, 255, 255).G; //green
                                    line[3 * x + 2] = img[x, y].hsvToRGB(imghsv[x, y].H, 255, 255).R; //red
                                    break;
                                } //hue
                            case "S":
                                {
                                    line[3 * x] = imghsv[x, y].S; //blue
                                    line[3 * x + 1] = imghsv[x, y].S; //green
                                    line[3 * x + 2] = imghsv[x, y].S; //red
                                    break;
                                } //saturation
                            case "V":
                                {
                                    line[3 * x] = imghsv[x, y].V;
                                    line[3 * x + 1] = imghsv[x, y].V;
                                    line[3 * x + 2] = imghsv[x, y].V;
                                    break;
                                } //value
                            case "CMYK":
                                {
                                    line[3 * x] = img[x, y].cmykToRGB(imgcmyk[x, y].C, imgcmyk[x, y].M, imgcmyk[x, y].Y, imgcmyk[x,y].K).B; //blue
                                    line[3 * x + 1] = img[x, y].cmykToRGB(imgcmyk[x, y].C, imgcmyk[x, y].M, imgcmyk[x, y].Y, imgcmyk[x, y].K).G; //green
                                    line[3 * x + 2] = img[x, y].cmykToRGB(imgcmyk[x, y].C, imgcmyk[x, y].M, imgcmyk[x, y].Y, imgcmyk[x, y].K).R; //red
                                    break;
                                }//cmyk
                            case "C":
                                {
                                    line[3 * x] = img[x, y].cmykToRGB(imgcmyk[x, y].C, 0, 0, 0).B; //blue
                                    line[3 * x + 1] = img[x, y].cmykToRGB(imgcmyk[x, y].C, 0, 0, 0).G; //green
                                    line[3 * x + 2] = img[x, y].cmykToRGB(imgcmyk[x, y].C, 0, 0, 0).R; //red
                                    break;
                                }//cyan
                            case "M":
                                {
                                    line[3 * x] = img[x, y].cmykToRGB(0, imgcmyk[x, y].M, 0, 0).B; //blue
                                    line[3 * x + 1] = img[x, y].cmykToRGB(0, imgcmyk[x, y].M, 0, 0).G; //green
                                    line[3 * x + 2] = img[x, y].cmykToRGB(0, imgcmyk[x, y].M, 0, 0).R; //red
                                    break;
                                }//magenta
                            case "Y":
                                {
                                    line[3 * x] = img[x, y].cmykToRGB(0, 0, imgcmyk[x, y].Y, 0).B; //blue
                                    line[3 * x + 1] = img[x, y].cmykToRGB(0, 0, imgcmyk[x, y].Y, 0).G; //green
                                    line[3 * x + 2] = img[x, y].cmykToRGB(0, 0, imgcmyk[x, y].Y, 0).R; //red
                                    break;
                                }//yellow
                            case "K":
                                {
                                    line[3 * x] = img[x, y].cmykToRGB(0, 0, 0, imgcmyk[x, y].K).B; //blue
                                    line[3 * x + 1] = img[x, y].cmykToRGB(0, 0, 0, imgcmyk[x, y].K).G; //green
                                    line[3 * x + 2] = img[x, y].cmykToRGB(0, 0, 0, imgcmyk[x, y].K).R; //red
                                    break;
                                }//key
                            case "YUV":
                                {
                                    line[3 * x] = img[x, y].yuvToRGB(imgyuv[x, y].Yy, imgyuv[x, y].U, imgyuv[x, y].Vv).B; //blue
                                    line[3 * x + 1] = img[x, y].yuvToRGB(imgyuv[x, y].Yy, imgyuv[x, y].U, imgyuv[x, y].Vv).G; //green
                                    line[3 * x + 2] = img[x, y].yuvToRGB(imgyuv[x, y].Yy, imgyuv[x, y].U, imgyuv[x, y].Vv).R; //red
                                    break;
                                }//yuv
                            case "Yy":
                                {
                                    line[3 * x] = img[x, y].yuvToRGB(imgyuv[x, y].Yy, 128, 128).B; //blue
                                    line[3 * x + 1] = img[x, y].yuvToRGB(imgyuv[x, y].Yy, 128, 128).G; //green
                                    line[3 * x + 2] = img[x, y].yuvToRGB(imgyuv[x, y].Yy, 128, 128).R; //red
                                    break;
                                }
                            case "U":
                                {
                                    line[3 * x] = img[x, y].yuvToRGB(128, imgyuv[x, y].U, 128).B; //blue
                                    line[3 * x + 1] = img[x, y].yuvToRGB(128, imgyuv[x, y].U, 128).G; //green
                                    line[3 * x + 2] = img[x, y].yuvToRGB(128, imgyuv[x, y].U, 128).R; //red
                                    break;
                                }
                            case "Vv":
                                {
                                    line[3 * x] = img[x, y].yuvToRGB(128, 128, imgyuv[x, y].Vv).B; //blue
                                    line[3 * x + 1] = img[x, y].yuvToRGB(128, 128, imgyuv[x, y].Vv).G; //green
                                    line[3 * x + 2] = img[x, y].yuvToRGB(128, 128, imgyuv[x, y].Vv).R; //red
                                    break;
                                }
                        } //switch
                    }
                    ptr = bmpData.Scan0 + y * bmpData.Stride;
                    Marshal.Copy(line, 0, ptr, line.Length);
                }
                bmp.UnlockBits(bmpData);
                watchdraw.Stop();
                var elapsedMs = watchdraw.ElapsedMilliseconds;
                Console.WriteLine("Image draw time " + elapsedMs);
                return bmp;
            }
            else
            {
                watchdraw.Stop();
                var elapsedMs = watchdraw.ElapsedMilliseconds;
                Console.WriteLine("Image draw time " + elapsedMs);
                return null;
            }
        }
    }
}
