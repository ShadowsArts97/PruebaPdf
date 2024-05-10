using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace PruebaPdf
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            float MaxWidth = 0, MaxHight = 0;
            List<byte[]> imagenByteList = new List<byte[]>();

            List<string> imagenes = new List<string>()
            {
                "C:\\Users\\PC01\\Pictures\\pruebaCed1.jpg",
                "C:\\Users\\PC01\\Pictures\\pruebaAntReg.jpg",
                "C:\\Users\\PC01\\Pictures\\poderEsp.jpg"
            };

            for (int i = 0; i < imagenes.Count; i++)
            {
                var bytes = File.ReadAllBytes(imagenes[i]);
                imagenByteList.Add(bytes);
            }

            Tuple<float, float> sizes = MaxSizePdf(imagenByteList);
            MaxWidth = sizes.Item1;
            MaxHight = sizes.Item2;
            var data = ConvertirImageAPdf(imagenByteList, 1280, 800);

            // iTextSharp.text.Image imagenPdf = iTextSharp.text.Image.GetInstance("C:\\Users\\PC01\\Pictures\\pruebaCed1.jpg");
            // var data = File.ReadAllBytes("C:\\Users\\PC01\\Pictures\\pruebaCed1.jpg");

            // System.Drawing.Image imagenSystemDrawing = ConvertToSystemDrawingImage(imagenPdf, data);
            // string base64 = ImageToBase64(data);
            string base64 = Convert.ToBase64String(data);

            Console.WriteLine(base64);
        }

        /// <summary>
        /// Metodo para generar el PDF unificado
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns>Documento PDF en byte[]</returns>
        public static byte[] ConvertirImageAPdf(List<byte[]> bytes, float Width, float Height)
        {
            byte[] result = null;
            iTextSharp.text.Rectangle pageSize = new iTextSharp.text.Rectangle(-1 * (Width + 100), -1 * (Height + 100), 0, 0);
            List<iTextSharp.text.Image> imagenes = new List<iTextSharp.text.Image>();

            for (int i = 0; i < bytes.Count; i++)
            {
                iTextSharp.text.Image image = GetImage(bytes[i], PageSize.LEGAL.Width, PageSize.LEGAL.Height);
                imagenes.Add(image);
            }

            using (MemoryStream stream = new MemoryStream())
            {
                using (Document document = new Document(PageSize.LEGAL, 5 ,5, 5 ,5 ))
                {
                    PdfWriter.GetInstance(document, stream);
                    document.Open();                    
                    foreach (iTextSharp.text.Image img in imagenes.OrderByDescending(x => x.Height))
                    {                        
                        float x = (document.PageSize.Width - img.Width) / 2;
                        float y = (document.PageSize.Height - img.Height) / 2;

                        // Agregar la imagen al documento en la posición calculada
                        img.SetAbsolutePosition(x, y);
                        var base64s = Convert.ToBase64String(img.RawData);
                        document.NewPage();
                        document.Add(img);
                    }
                }
                result = stream.ToArray();
                var base64 = Convert.ToBase64String(result);
            }
            return result;
        }

        /// <summary>
        /// Metodo para Extraer la image mas grande para el PDF
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>Retorna Tupla con Valores de float de Ancho y Alto del PDF</returns>
        public static Tuple<float, float> MaxSizePdf(List<byte[]> bytes)
        {
            List<Tuple<float, float>> Sizes = new List<Tuple<float, float>>();
            float Width = 0, Height = 0;
            for (int i = 0; i < bytes.Count; i++)
            {
                byte[] data = bytes[i];
                using (MemoryStream stream = new MemoryStream(data))
                {
                    using (System.Drawing.Image image = new Bitmap(stream))
                    {
                        Width = image.Width;
                        Height = image.Height;
                        Sizes.Add(Tuple.Create(Width, Height));
                    }
                }
            }
            Width = Sizes.Max(x => x.Item1);
            Height = Sizes.Max(x => x.Item2);
            return Tuple.Create(Width, Height);
        }

        public static System.Drawing.Image ConvertToSystemDrawingImage(iTextSharp.text.Image imagenPdf, byte[] bytes = null)
        {
            byte[] resultado = null;
            iTextSharp.text.Image image2 = ImagenPdf(bytes, 900);
            var imgeb = Convert.ToBase64String(image2.RawData);
            System.Drawing.Image image = null;
            float Width = -1 * (image2.ScaledWidth), Height = -1 * (image2.ScaledHeight);
            using (MemoryStream ms = new MemoryStream())
            {
                iTextSharp.text.Rectangle pageSize = new iTextSharp.text.Rectangle(Width, Height, 0, 0);
                using (Document doc = new Document(pageSize, 0, 0, 0, 0))
                {
                    PdfWriter.GetInstance(doc, ms);
                    doc.Open();
                    doc.Add(image2);
                }
                resultado = ms.ToArray();
                var base64 = Convert.ToBase64String(resultado);
            }
            using (Stream stream = null)
            {
                var largo = resultado.Length;
                stream.Write(resultado, 0, largo);
                image = System.Drawing.Image.FromStream(stream);
            }
            return image;
        }
        /// <summary>
        /// Obtener Imagen Reescalada
        /// </summary>
        /// <param name="data"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        public static iTextSharp.text.Image GetImage(byte[] data, float Width, float Height)
        {
            iTextSharp.text.Image ResultImagen = null;
            System.Drawing.Image ReturnBitmap = null;
            using (MemoryStream memoryStream = new MemoryStream(data))
            {
                memoryStream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (System.Drawing.Image R_ImagenBitMap = new Bitmap(memoryStream))
                {
                    //if (R_ImagenBitMap.Width < (int)Width || R_ImagenBitMap.Height < (int)Height)
                        ReturnBitmap = EscalarImageV2(R_ImagenBitMap, (int)Width, (int)Height);
                    //else
                    //    ReturnBitmap = R_ImagenBitMap;
                    try
                    {
                        ResultImagen = iTextSharp.text.Image.GetInstance(ReturnBitmap, ImageFormat.Jpeg);
                    }
                    catch (Exception e)
                    {
                        ReturnBitmap = R_ImagenBitMap;
                        ResultImagen = iTextSharp.text.Image.GetInstance(ReturnBitmap, ReturnBitmap.RawFormat);
                    }

                    R_ImagenBitMap.Dispose();
                }
                memoryStream.Flush();
                memoryStream.Position = 0;
                memoryStream.Close();
                memoryStream.Dispose();
            }
            return ResultImagen;
        }

        public static iTextSharp.text.Image ImagenPdf(byte[] bytes, int MaxSize)
        {
            iTextSharp.text.Image R_Imagen_PDF = null;
            System.Drawing.Image ReturnBitmap = null;
            using (MemoryStream memoryStream = new MemoryStream(bytes))
            {
                memoryStream.Flush();
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (System.Drawing.Image R_ImagenBitMap = new Bitmap(memoryStream))
                {
                    if (MaxSize == -1)
                        ReturnBitmap = R_ImagenBitMap;

                    if (R_ImagenBitMap.Width < MaxSize || R_ImagenBitMap.Height < MaxSize)
                        ReturnBitmap = EscalarImagen(R_ImagenBitMap, MaxSize);
                    else if (R_ImagenBitMap.Width < 500)
                        ReturnBitmap = EscalarImagen(R_ImagenBitMap, 500);
                    else
                        ReturnBitmap = R_ImagenBitMap;

                    try
                    {
                        R_Imagen_PDF = iTextSharp.text.Image.GetInstance(ReturnBitmap, ImageFormat.Jpeg);
                    }
                    catch (Exception e)
                    {
                        ReturnBitmap = R_ImagenBitMap;
                        R_Imagen_PDF = iTextSharp.text.Image.GetInstance(ReturnBitmap, ReturnBitmap.RawFormat);
                    }

                    R_ImagenBitMap.Dispose();
                }
                memoryStream.Flush();
                memoryStream.Position = 0;
                memoryStream.Close();
                memoryStream.Dispose();
            }
            return R_Imagen_PDF;
        }

        /// <summary>
        /// Metodo para Escalar la Image de Manera Homogenea al Pdf
        /// </summary>
        /// <param name="ImageOriginal"></param>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        private static System.Drawing.Image EscalarImageV2(System.Drawing.Image ImageOriginal, int Width, int Height)
        {
            Bitmap bitmap = null;
            double ratioX = (double)Width / ImageOriginal.Width;
            double ratioY = (double)Height / ImageOriginal.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(ImageOriginal.Width * ratio);
            int newHeight = (int)(ImageOriginal.Height * ratio);
            try
            {
                var newImage = new Bitmap(newWidth, newHeight);
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.DrawImage(ImageOriginal, 0, 0, newWidth, newHeight);
                    bitmap = newImage;
                    //(Bitmap)ImageOriginal;
                    // ImageOriginal.Dispose();
                }
                return bitmap;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static System.Drawing.Image EscalarImagen(System.Drawing.Image ImageOriginal, int MaxSize)
        {
            Bitmap bitmap;
            if (ImageOriginal == null || MaxSize <= 0)
            {
                return null;
            }

            double ratioX = (double)MaxSize / ImageOriginal.Width;
            double ratioY = (double)MaxSize / ImageOriginal.Height;
            double ratio = Math.Min(ratioX, ratioY);

            int newWidth = (int)(ImageOriginal.Width * ratio);
            int newHeight = (int)(ImageOriginal.Height * ratio);

            try
            {
                var newImage = new Bitmap(newWidth, newHeight);
                using (var graphics = Graphics.FromImage(newImage))
                {
                    graphics.DrawImage(ImageOriginal, 0, 0, newWidth, newHeight);
                    bitmap = newImage;//(Bitmap)ImageOriginal;
                                      // ImageOriginal.Dispose();
                }

                return bitmap;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static string ImageToBase64(System.Drawing.Image image)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                // Convierte la imagen a bytes
                image.Save(ms, image.RawFormat);
                byte[] imageBytes = ms.ToArray();

                // Convierte los bytes a base64
                string base64String = Convert.ToBase64String(imageBytes);
                return base64String;
            }
        }
    }
}