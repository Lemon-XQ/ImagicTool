using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagicTool
{
    class ImageHelper
    {
        // 按指定尺寸对图像pic进行非拉伸缩放
        public static Bitmap shrinkTo(Image pic, Size S, Boolean cutting) {
            //创建图像
            Bitmap tmp = new Bitmap(S.Width, S.Height);     //按指定大小创建位图
            //绘制
            Graphics g = Graphics.FromImage(tmp);           //从位图创建Graphics对象
            g.Clear(Color.FromArgb(0, 0, 0, 0));            //清空
            Boolean mode = (float)pic.Width / S.Width > (float)pic.Height / S.Height;   //zoom缩放
            if (cutting) mode = !mode;                      //裁切缩放

            //计算Zoom绘制区域             
            if (mode)
                S.Height = (int)((float)pic.Height * S.Width / pic.Width);
            else
                S.Width = (int)((float)pic.Width * S.Height / pic.Height);
            Point P = new Point((tmp.Width - S.Width) / 2, (tmp.Height - S.Height) / 2);
            g.DrawImage(pic, new Rectangle(P, S));
            return tmp;     //返回构建的新图像
        }

        // 将彩色图转化为灰度图
        public static Bitmap rgb2gray(Bitmap bitmap_before) {
            Bitmap gray_bitmap = bitmap_before.Clone() as Bitmap;
            Color pixel;
            int gray;
            for (int x = 0; x < gray_bitmap.Width; x++) {
                for (int y = 0; y < gray_bitmap.Height; y++) {
                    pixel = gray_bitmap.GetPixel(x, y);
                    gray = (int)(0.3 * pixel.R + 0.59 * pixel.G + 0.11 * pixel.B);
                    gray_bitmap.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                }
            }
            return gray_bitmap;
        }

        /// <summary>
        /// 图片添加文字，文字大小自适应
        /// </summary>
        /// <param name="imgPath">图片路径</param>
        /// <param name="locationLeftTop">左上角定位(x1,y1)</param>
        /// <param name="locationRightBottom">右下角定位(x2,y2)</param>
        /// <param name="text">文字内容</param>
        /// <param name="fontName">字体名称</param>
        /// <returns>添加文字后的Bitmap对象</returns>
        public static Bitmap AddText(Bitmap img, string locationLeftTop, string locationRightBottom, 
                                     string text, string fontName = "华文隶书") {
            int width = img.Width;
            int height = img.Height;
            Bitmap bmp = new Bitmap(width, height);
            Graphics graph = Graphics.FromImage(bmp);
            // 计算文字区域
            // 左上角
            string[] location = locationLeftTop.Split(',');
            float x1 = float.Parse(location[0]);
            float y1 = float.Parse(location[1]);
            // 右下角
            location = locationRightBottom.Split(',');
            float x2 = float.Parse(location[0]);
            float y2 = float.Parse(location[1]);
            // 区域宽高
            float fontWidth = x2 - x1;
            float fontHeight = y2 - y1;

            float fontSize = fontHeight;  // 初次估计先用文字区域高度作为文字字体大小，后面再做调整，单位为px

            Font font = new Font(fontName, fontSize, GraphicsUnit.Pixel);// 字体对象
            SizeF sf = graph.MeasureString(text, font);

            // 调整字体大小以适应文字区域
            if (sf.Width > fontWidth) {
                while (sf.Width > fontWidth) {
                    fontSize -= 0.1f;
                    font = new Font(fontName, fontSize, GraphicsUnit.Pixel);
                    sf = graph.MeasureString(text, font);
                }
            } else if (sf.Width < fontWidth) {
                while (sf.Width < fontWidth) {
                    fontSize += 0.1f;
                    font = new Font(fontName, fontSize, GraphicsUnit.Pixel);
                    sf = graph.MeasureString(text, font);
                }
            }
            // 最终的得出的字体所占区域一般不会刚好等于实际区域
            // 所以根据两个区域的相差之处再把文字开始位置(左上角定位)稍微调整一下
            x1 += (fontWidth - sf.Width) / 2;
            y1 += (fontHeight - sf.Height) / 2;
            // 绘制图像及文字
            graph.DrawImage(img, 0, 0, width, height);
            graph.DrawString(text, font, new SolidBrush(Color.Black), x1, y1);
            graph.Dispose();
            return bmp;
        }
    }
}
