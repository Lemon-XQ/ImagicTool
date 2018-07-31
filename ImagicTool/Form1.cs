using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImagicTool
{
    public partial class Form1 : Form
    {
        public Form1() {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage; //设置picturebox为居中模式
            pictureBox2.SizeMode = PictureBoxSizeMode.CenterImage; //设置picturebox为居中模式
        }

        Bitmap bitmap_before;// 未处理的bitmap图像
        Bitmap bitmap_after;// 处理后的bitmap图像

        #region 文件操作：打开、保存、退出
        // 打开图片文件
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (openFileDialog1.ShowDialog() == DialogResult.OK) {
                string path = openFileDialog1.FileName;// 读取所选图片的路径
                bitmap_before = (Bitmap)Image.FromFile(path);// 转为bitmap
                bitmap_after = bitmap_before;
                Image img_before = bitmap_before.Clone() as Image;// 转为Image
                Image img_after = img_before;
                pictureBox1.Image = img_before;// 显示图像
                pictureBox2.Image = img_before;// 显示图像
                // 初始化所有滑动条
                trackBar1_Scroll(sender, e);// 使图像等比例缩放
                trackBar2_Scroll(sender, e);// 使图像等比例缩放
                trackBar1.Value = 5;
                trackBar2.Value = 5;
                trackBar3.Value = 0;
                trackBar4.Value = 0;
                trackBar5.Value = 0;
                // 隐藏文字面板
                textPanel.Visible = false;
                // 重置边框
                pictureBox2.Region = new Region();
            }
        }

        // 保存处理后的图像
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e) {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK) {
                string fileName = saveFileDialog1.FileName.ToString();// 读取保存路径
                if (fileName != "" && fileName != null) {
                    string suffix = fileName.Substring(fileName.LastIndexOf(".") + 1).ToString();// 提取文件后缀
                    ImageFormat imgformat = null;
                    // 根据后缀保存为相应格式的图片
                    if (suffix != "") {
                        switch (suffix) {
                            case "jpg":
                                imgformat = ImageFormat.Jpeg;
                                break;
                            case "png":
                                imgformat = ImageFormat.Png;
                                break;
                            case "bmp":
                                imgformat = ImageFormat.Bmp;
                                break;
                            case "gif":
                                imgformat = ImageFormat.Gif;
                                break;
                            default:
                                MessageBox.Show("ERROR: 只能保存为jpg/png/bmp/gif格式");
                                break;
                        }
                    }
                    // 无指定后缀时默认保存为JPG格式   
                    if (imgformat == null)  imgformat = ImageFormat.Jpeg;
                    // 写入文件
                    try {
                        this.pictureBox2.Image.Save(fileName, imgformat);  
                    } catch {
                        MessageBox.Show("保存失败，图片为空!");
                    }
                }
            }
        }

        // 退出程序
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e) {
            Application.Exit();
        }
        #endregion

        // 控制左图的缩放
        private void trackBar1_Scroll(object sender, EventArgs e) {
            double zoomFactor = 1; // 缩放因子         
            if (trackBar1.Value == 5) {// 不变
                zoomFactor = 1;
            }else if(trackBar1.Value > 5 ){// 放大
                zoomFactor = Convert.ToDouble(1 + (trackBar1.Value-5) * 0.1);
            } else {// 缩小
                zoomFactor = Convert.ToDouble(1 - (5-trackBar1.Value) * 0.1);
            }
            int pwidth = pictureBox1.Width;
            int pheight = pictureBox1.Height;
            int width = Convert.ToInt32(pwidth * zoomFactor);
            int height = Convert.ToInt32(pheight * zoomFactor);
            // 根据缩放因子得到缩放后的图像
            Bitmap bitmap_zoom = ImageHelper.shrinkTo(bitmap_before, new Size(width, height), false);
            pictureBox1.Image = bitmap_zoom.Clone() as Image;
        }

        // 控制右图的缩放
        private void trackBar2_Scroll(object sender, EventArgs e) {
            double zoomFactor = 1; // 缩放因子         
            if (trackBar2.Value == 5) {// 不变
                zoomFactor = 1;
            } else if (trackBar2.Value > 5) {// 放大
                zoomFactor = Convert.ToDouble(1 + (trackBar2.Value - 5) * 0.1);
            } else {// 缩小
                zoomFactor = Convert.ToDouble(1 - (5 - trackBar2.Value) * 0.1);
            }
            int pwidth = pictureBox1.Width;
            int pheight = pictureBox1.Height;
            int width = Convert.ToInt32(pwidth * zoomFactor);
            int height = Convert.ToInt32(pheight * zoomFactor);
            // 根据缩放因子得到缩放后的图像
            Bitmap bitmap_zoom = ImageHelper.shrinkTo(bitmap_after, new Size(width, height), false);
            pictureBox2.Image = bitmap_zoom.Clone() as Image;
        }

        // 调节亮度
        private void trackBar3_Scroll(object sender, EventArgs e) {
            double zoomFactor; // 缩放因子    
            // 滑动条value=0时亮度不变，<0时变暗，>0时变亮
            zoomFactor = Convert.ToDouble(1 + trackBar3.Value * 0.05);
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;// 在处理后的图上做出的调节
                Color pixel;
                int r, g, b;
                for (int x = 0; x < bitmap_after.Width; x++) {
                    for (int y = 0; y < bitmap_after.Height; y++) {
                        pixel = bitmap_after.GetPixel(x, y);
                        // 通过等比例改变RGB分量实现亮度调节
                        r = (int)(pixel.R * zoomFactor);
                        g = (int)(pixel.G * zoomFactor);
                        b = (int)(pixel.B * zoomFactor);
                        // 防止颜色值超过255
                        r = r > 255 ? 255 : r;
                        g = g > 255 ? 255 : g;
                        b = b > 255 ? 255 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 调整对比度（仿Photoshop算法）
        private void trackBar4_Scroll(object sender, EventArgs e) {
            double zoomFactor; // 缩放因子    
            // 滑动条value=0时对比度不变，<0时对比度变低，>0时对比度变高
            zoomFactor = Convert.ToDouble(trackBar4.Value * 0.05);
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;// 每次都是在原图上做出的调节
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel;
                int r, g, b;
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        pixel = bitmap_after.GetPixel(x, y);
                        int average = 127;// 当前通道均值，统一取127
                        if (zoomFactor >= 0) {// 增加对比度
                            r = (int)(average + (pixel.R - average) / (1 - zoomFactor));
                            g = (int)(average + (pixel.G - average) / (1 - zoomFactor));
                            b = (int)(average + (pixel.B - average) / (1 - zoomFactor));
                        } else {// 降低对比度
                            r = (int)(average + (pixel.R - average) * (1 + zoomFactor));
                            g = (int)(average + (pixel.G - average) * (1 + zoomFactor));
                            b = (int)(average + (pixel.B - average) * (1 + zoomFactor));
                        }
                        // 防止颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 调整色彩饱和度（仿Photoshop算法）
        private void trackBar5_Scroll(object sender, EventArgs e) {
            double zoomFactor; // 缩放因子    
            zoomFactor = Convert.ToDouble(trackBar5.Value * 0.05);
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;// 每次都是在原图上做出的调节
                Color pixel;
                int r, g, b;
                for (int x = 0; x < bitmap_after.Width; x++) {
                    for (int y = 0; y < bitmap_after.Height; y++) {
                        pixel = bitmap_after.GetPixel(x, y);
                        // 利用HSL模式求得颜色的S和L
                        double rgbMax = Math.Max(Math.Max(pixel.R,pixel.G),pixel.B);
                        double rgbMin = Math.Min(Math.Min(pixel.R, pixel.G), pixel.B);
                        double delta = (rgbMax - rgbMin) / 255;
                        // 如果delta=0，S=0，所以不能调整饱和度
                        if (delta == 0) continue;

                        double value = (rgbMax + rgbMin) / 255;
                        double S;// HSL中的S——饱和度
                        double L = value / 2;// HSL中的L——亮度
                        // 根据L的范围求饱和度S
                        if (L < 0.5)
                            S = delta / value;
                        else
                            S = delta / (2 - value);
                        // 饱和度调整：如果zoomFactor>0，饱和度呈级数增强，否则线性衰减
                        double alpha;// 增减量
                        if (zoomFactor >= 0) {// 增加饱和度
                            // 如果zoomFactor+S > 1，用S代替增减量，以控制饱和度的上限
                            if (zoomFactor + S >= 1)
                                alpha = S;
                            else// 否则取zoomFactor的补数
                                alpha = 1 - zoomFactor;
                            // 求倒数 - 1，实现级数增强
                            alpha = 1 / alpha - 1;
                            r = (int)(pixel.R + (pixel.R - L * 255) * alpha);
                            g = (int)(pixel.G + (pixel.G - L * 255) * alpha);
                            b = (int)(pixel.B + (pixel.B - L * 255) * alpha);
                        } else {// 降低饱和度
                            alpha = zoomFactor;
                            r = (int)(L * 255 + (pixel.R - L * 255) * (1 + alpha));
                            g = (int)(L * 255 + (pixel.G - L * 255) * (1 + alpha));
                            b = (int)(L * 255 + (pixel.B - L * 255) * (1 + alpha));
                        }
                        // 防止颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效1：胶片暗角
        // 原理：根据像素与中心的距离重新计算颜色值，四个顶点与中心距离最远，颜色为黑色
        private void button1_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                float cx = width / 2;// 中心x坐标
                float cy = height / 2;// 中心y坐标
                float maxDist = cx * cx + cy * cy;// 四个顶点与中心的距离
                float currDist = 0;// 每个像素点与中心的距离
                float factor;// 调节因子，factor = distance / maxDistance
                Color pixel;
                for (int i = 0; i < width; i++) {
                    for (int j = 0; j < height; j++) {
                        // 计算每个像素与中心的距离
                        currDist = ((float)i - cx) * ((float)i - cx) + ((float)j - cy) * ((float)j - cy);
                        factor = currDist / maxDist;
                        pixel = bitmap_after.GetPixel(i, j);
                        // 重新计算RGB值
                        int r = (int)(pixel.R * (1 - factor));
                        int g = (int)(pixel.G * (1 - factor));
                        int b = (int)(pixel.B * (1 - factor));
                        bitmap_after.SetPixel(i, j, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效2：黑白照片
        // 原理：把当前像素点的颜色按下面的公式的调整 gray = 0.3 * R + 0.59 * G + 0.11 * B
        private void button2_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                Color pixel;
                int gray;
                for (int x = 0; x < bitmap_after.Width; x++) {
                    for (int y = 0; y < bitmap_after.Height; y++) {
                        pixel = bitmap_after.GetPixel(x, y);
                        gray = (int)(0.3 * pixel.R + 0.59 * pixel.G + 0.11 * pixel.B);
                        bitmap_after.SetPixel(x, y, Color.FromArgb(gray, gray, gray));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效3：复古风
        // 原理：使用老照片矩阵重新计算RGB分量，矩阵每一行之和都为1，每个点的rgb值都是原来rgb值按照这个比例实现的。
        //    |0.393 0.769 0.189|
        // M =|0.349 0.686 0.168|
        //    |0.272 0.534 0.131|
        private void button3_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                Color pixel;
                int r, g, b;
                for (int x = 0; x < bitmap_after.Width; x++) {
                    for (int y = 0; y < bitmap_after.Height; y++) {
                        pixel = bitmap_after.GetPixel(x, y);
                        r = (int)(0.393 * pixel.R + 0.769 * pixel.G + 0.189 * pixel.B);
                        g = (int)(0.349 * pixel.R + 0.686 * pixel.G + 0.168 * pixel.B);
                        b = (int)(0.272 * pixel.R + 0.534 * pixel.G + 0.131 * pixel.B);
                        // 防止颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效4：马赛克
        // 原理：把一个像素点周围的点的像素取平均，然后把这些像素点的颜色设为这个平均值
        private void button4_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int RIDIO = 20;//马赛克的尺度，越大则越糊
                for (int h = 0; h < bitmap_after.Height; h += RIDIO) {
                    for (int w = 0; w < bitmap_after.Width; w += RIDIO) {
                        int avgRed = 0, avgGreen = 0, avgBlue = 0;
                        int count = 0;
                        //取周围的像素
                        for (int x = w; (x < w + RIDIO && x < bitmap_after.Width); x++) {
                            for (int y = h; (y < h + RIDIO && y < bitmap_after.Height); y++) {
                                Color pixel = bitmap_after.GetPixel(x, y);
                                avgRed += pixel.R;
                                avgGreen += pixel.G;
                                avgBlue += pixel.B;
                                count++;
                            }
                        }
                        //取平均值
                        avgRed = avgRed / count;
                        avgBlue = avgBlue / count;
                        avgGreen = avgGreen / count;
                        //设置颜色
                        for (int x = w; (x < w + RIDIO && x < bitmap_after.Width); x++) {
                            for (int y = h; (y < h + RIDIO && y < bitmap_after.Height); y++) {
                                Color newColor = Color.FromArgb(avgRed, avgGreen, avgBlue);
                                bitmap_after.SetPixel(x, y, newColor);
                            }
                        }
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效5：底片效果
        // 原理：RGB三分量取反
        private void button5_Click(object sender, EventArgs e) {
            if(bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel;
                for (int x = 1; x < width; x++) {
                    for (int y = 1; y < height; y++) {
                        int r, g, b;
                        pixel = bitmap_before.GetPixel(x, y);
                        // 取反
                        r = 255 - pixel.R;
                        g = 255 - pixel.G;
                        b = 255 - pixel.B;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效6：浮雕效果
        // 原理: 对图像像素点的像素值分别与相邻像素点的像素值相减后加上128, 然后将其作为新的像素点的值.
        private void button6_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel1, pixel2;
                for (int x = 1; x < width - 1; x++) {
                    for (int y = 1; y < height - 1; y++) {
                        int r, g, b;
                        pixel1 = bitmap_before.GetPixel(x, y);
                        pixel2 = bitmap_before.GetPixel(x + 1, y + 1);// 取相邻像素
                        r = Math.Abs(pixel1.R - pixel2.R + 128);
                        g = Math.Abs(pixel1.G - pixel2.G + 128);
                        b = Math.Abs(pixel1.B - pixel2.B + 128);
                        // 防止颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效7：柔化效果
        // 原理：使用高斯模板对图像进行卷积，在当前像素点与周围像素点的颜色差距较大时取其平均值
        private void button9_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel;
                //高斯模板
                int[] Gauss = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };
                for (int x = 1; x < width - 1; x++) {
                    for (int y = 1; y < height - 1; y++) {
                        int r = 0, g = 0, b = 0;
                        int Index = 0;
                        // 对每个像素应用高斯模板重新计算RGB分量值
                        for (int col = -1; col <= 1; col++)
                            for (int row = -1; row <= 1; row++) {
                                pixel = bitmap_before.GetPixel(x + row, y + col);
                                r += pixel.R * Gauss[Index];
                                g += pixel.G * Gauss[Index];
                                b += pixel.B * Gauss[Index];
                                Index++;
                            }
                        // 取平均值
                        r /= 16;
                        g /= 16;
                        b /= 16;
                        //处理颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        //特效8：锐化效果
        //原理：使用拉普拉斯模板对图像进行卷积，突出显示颜色值大(即形成形体边缘)的像素点
        private void button10_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel;
                //拉普拉斯模板
                int[] Laplacian = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };
                for (int x = 1; x < width - 1; x++) {
                    for (int y = 1; y < height - 1; y++) {
                        int r = 0, g = 0, b = 0;
                        int Index = 0;// 记录拉普拉斯模板的索引值
                        // 对每个像素应用拉普拉斯模板重新计算RGB分量值
                        for (int col = -1; col <= 1; col++)
                            for (int row = -1; row <= 1; row++) {
                                pixel = bitmap_before.GetPixel(x + row, y + col);
                                r += pixel.R * Laplacian[Index];
                                g += pixel.G * Laplacian[Index];
                                b += pixel.B * Laplacian[Index];
                                Index++;
                            }
                        //处理颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        //特效9：素描效果
        //原理：去色->高斯模糊->颜色减淡
        private void button11_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel;
                Bitmap gray = ImageHelper.rgb2gray(bitmap_before);// 灰度图
                Bitmap reverse = bitmap_before.Clone() as Bitmap;// 反色后的图像
                Bitmap gauss = bitmap_before.Clone() as Bitmap;// 高斯模糊后的图像
                int r = 0, g = 0, b = 0;
                // 求得反色后的图像reverse
                for(int x = 1; x < width; x++) {
                    for(int y = 1; y < height; y++) {
                        pixel = gray.GetPixel(x, y);
                        r = pixel.R;
                        g = pixel.G;
                        b = pixel.B;
                        // 反色，RGB分量取反
                        reverse.SetPixel(x, y, Color.FromArgb(255 - r, 255 - g, 255 - b));
                    }          
                }
                // 高斯模糊
                int[] Gauss = { 1, 2, 1, 2, 4, 2, 1, 2, 1 };// 高斯模板
                for (int i = 2; i < width - 1;i++)
                    for(int j = 2; j < height - 1; j++) {
                        int sum = 0;
                        int Index = 0;
                        // 对每个像素应用高斯模板重新计算RGB分量值
                        for (int col = -1; col <= 1; col++)
                            for (int row = -1; row <= 1; row++) {
                                pixel = reverse.GetPixel(i + row, j + col);
                                sum += pixel.R * Gauss[Index];
                                Index++;
                            }
                        sum = sum / 16;
                        gauss.SetPixel(i, j, Color.FromArgb(sum, sum, sum));
                    }
                // 颜色减淡，公式C=MIN( A +（A×B）/（256-B）,255)，其中C为混合结果，A为源像素点，B为目标像素点
                for (int i = 1; i < width; i++) 
                    for(int j = 1;j < height; j++) {
                        int B = gauss.GetPixel(i, j).R;
                        int A = gray.GetPixel(i, j).R;
                        int C = Math.Min(A + A * B / (256 - B), 255);
                        bitmap_after.SetPixel(i, j, Color.FromArgb(C, C, C));
                    }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效10：放大镜，实现局部放大
        // 原理：放大镜的坐标为(CenterX，CenterY)，半径为Radius，放大倍数为factor，那么就是将原图中的坐标为(CenterX，CenterY)、半径为Radius/M的区域的图像放大到放大镜覆盖的区域即可
        // 算法：对图片上的每一个点(X,Y)，求其与(CenterX，CenterY)的距离Distance，若Distance < Radius，则取原图中坐标为(X/factor,Y/factor)的像素的颜色值作为新的颜色值。
        private void button13_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel;
                int centerX = width / 2, centerY = height / 2;// 放大中心
                int radius = Math.Min(height,width)/2;// 放大半径
                int factor = 2;// 放大倍数
                for (int x = 1; x < width - 1; x++) {
                    for (int y = 1; y < height - 1; y++) {
                        int r = 0, g = 0, b = 0;
                        // 计算每一个点与放大中心的距离
                        int distance = (int)((centerX - x) * (centerX - x) + (centerY - y) * (centerY - y));
                        // 判断是否在放大半径范围内
                        if (distance < radius * radius) {
                            // 图像放大效果  
                            int src_x = (int)((float)(x - centerX) / factor + centerX);
                            int src_y = (int)((float)(y - centerY) / factor + centerY);
                            pixel = bitmap_before.GetPixel(src_x, src_y);
                        } else {
                            pixel = bitmap_before.GetPixel(x, y);// 保持原有像素值不变
                        }
                        r = pixel.R;
                        g = pixel.G;
                        b = pixel.B;
                        //处理颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效11：哈哈镜，实现图像扭曲
        // 原理：在放大镜的基础上做出调整，越靠近中心的点放大倍数越大，靠近边缘的点放大倍数越小，以此形成扭曲的效果
        private void button12_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel;
                int centerX = width / 2, centerY = height / 2;// 放大中心
                int radius = Math.Min(height, width) / 2;// 放大半径
                int factor = 2;// 放大倍数
                for (int x = 1; x < width - 1; x++) {
                    for (int y = 1; y < height - 1; y++) {
                        int r = 0, g = 0, b = 0;
                        // 计算每一个点与放大中心的距离
                        int distance = (int)((centerX - x) * (centerX - x) + (centerY - y) * (centerY - y));
                        // 判断是否在放大半径范围内
                        if (distance < radius * radius) {
                            // 图像凹凸效果  
                            int src_x = (int)((float)(x - centerX) / factor);
                            int src_y = (int)((float)(y - centerY) / factor);
                            // 越靠近中心的点放大倍数越大
                            src_x = (int)(src_x * (Math.Sqrt(distance) / radius));
                            src_y = (int)(src_y * (Math.Sqrt(distance) / radius));
                            src_x = src_x + centerX;
                            src_y = src_y + centerY;
                            pixel = bitmap_before.GetPixel(src_x, src_y);
                        } else {
                            pixel = bitmap_before.GetPixel(x, y);// 保持原有像素值不变
                        }
                        r = pixel.R;
                        g = pixel.G;
                        b = pixel.B;
                        //处理颜色值溢出
                        r = r > 255 ? 255 : r;
                        r = r < 0 ? 0 : r;
                        g = g > 255 ? 255 : g;
                        g = g < 0 ? 0 : g;
                        b = b > 255 ? 255 : b;
                        b = b < 0 ? 0 : b;
                        bitmap_after.SetPixel(x, y, Color.FromArgb(r, g, b));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 特效12：油画效果
        // 原理：基于像素权重实现图像的像素模糊从而达到近似油画效果模糊
        private void button14_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                bitmap_after = bitmap_before.Clone() as Bitmap;
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Color pixel;
                int radius = 2;// 模糊半径
                int intensity = 20;// 模糊强度
                int[] intensityCount = new int[intensity + 1];// 统计各个灰度等级的数目
                // 各个灰度等级对应的RGB分量的平均值
                int[] ravg = new int[intensity + 1];
                int[] gavg = new int[intensity + 1];
                int[] bavg = new int[intensity + 1];
                int r = 0, g = 0, b = 0;
                for (int x = 0; x < width; x++) {
                    for (int y = 0; y < height; y++) {
                        // 初始化数组为0
                        for (int i = 0; i <= intensity; i++) {
                            intensityCount[i] = 0;
                            ravg[i] = 0;
                            gavg[i] = 0;
                            bavg[i] = 0;
                        }
                        // 对模糊半径内的像素进行处理
                        for (int sub_y = -radius; sub_y <= radius; sub_y++) {
                            for (int sub_x = -radius; sub_x <= radius; sub_x++) {
                                int nrow = y + sub_y;
                                int ncol = x + sub_x;
                                // 防止溢出
                                if (nrow >= height || nrow < 0) {
                                    nrow = 0;
                                }
                                if (ncol >= width || ncol < 0) {
                                    ncol = 0;
                                }
                                pixel = bitmap_before.GetPixel(ncol, nrow);// 取得当前像素
                                r = pixel.R;
                                g = pixel.G;
                                b = pixel.B;
                                // 计算模糊后的灰度强度
                                int curIntensity = (int)(((double)((r + g + b) / 3) * intensity) / 255.0f);
                                intensityCount[curIntensity]++;// 该灰度强度下的像素数+1
                                // 该灰度强度下的所有像素的RGB分量累加
                                ravg[curIntensity] += r;
                                gavg[curIntensity] += g;
                                bavg[curIntensity] += b;
                            }
                        }
                        // 找到像素数目最多的灰度等级及其索引
                        int maxCount = 0, maxIndex = 0;
                        for (int m = 0; m < intensityCount.Length; m++) {
                            if (intensityCount[m] > maxCount) {
                                maxCount = intensityCount[m];
                                maxIndex = m;
                            }
                        }
                        // 得到像素RGB分量平均值
                        int nr = ravg[maxIndex] / maxCount;
                        int ng = gavg[maxIndex] / maxCount;
                        int nb = bavg[maxIndex] / maxCount;
                        // 重新设置RGB分量
                        bitmap_after.SetPixel(x, y, Color.FromArgb(nr, ng, nb));
                    }
                }
                pictureBox2.Image = bitmap_after;
                trackBar2_Scroll(sender, e);// 调整缩放比例
            }
        }

        // 对左图进行旋转
        private void button7_Click(object sender, EventArgs e) {
            if (bitmap_before != null) {
                Image temp = pictureBox1.Image.Clone() as Image;// 拷贝原图
                temp.RotateFlip(RotateFlipType.Rotate90FlipNone);// 翻转
                pictureBox1.Image = temp;
            }
                
        }

        // 对右图进行旋转
        private void button8_Click(object sender, EventArgs e) {
            if (bitmap_after != null) {
                Image temp = pictureBox2.Image.Clone() as Image;// 拷贝原图
                temp.RotateFlip(RotateFlipType.Rotate90FlipNone);// 翻转
                pictureBox2.Image = temp;
            }
        }

        // Reset，清除所有效果
        private void button15_Click(object sender, EventArgs e) {
            if (bitmap_after != null) {
                pictureBox2.Image = pictureBox1.Image.Clone() as Image;
                bitmap_after = bitmap_before;
                textPanel.Visible = false;
                pictureBox2.Region = new Region();
            }
        }

        // DIY：添加文字
        private void button27_Click(object sender, EventArgs e) {
            if(bitmap_after != null) {
                textPanel.Visible = true;// 开启文字编辑面板
            }
        }

        // 确认添加文字
        private void button18_Click(object sender, EventArgs e) {
            // 在图片下方加上文字
            bitmap_after = ImageHelper.AddText(bitmap_after,"110,150","200,420",textBox1.Text);
            pictureBox2.Image = bitmap_after;
            textPanel.Visible = false;// 隐藏文字编辑面板
        }

        // DIY：添加星形边框
        private void button26_Click(object sender, EventArgs e) {
            if(bitmap_after != null) {
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bitmap);// 得到Graphics对象

                Pen p = new Pen(Color.Black, 2);//声明一个画笔,黑色，笔刷大小为2
                Brush b = new TextureBrush(bitmap_after);// 创建图像笔刷
                // 星形的10个顶点
                Point p1 = new Point(150, 0);
                Point p2 = new Point(120, 100);
                Point p3 = new Point(0, 100);
                Point p4 = new Point(90, 200);
                Point p5 = new Point(30, 300);
                Point p6 = new Point(150, 250);
                Point p7 = new Point(270, 300);
                Point p8 = new Point(210, 200);
                Point p9 = new Point(300, 100);
                Point p10 = new Point(180, 100);
                Point[] points = { p1, p2, p3, p4, p5, p6, p7, p8, p9, p10 };
                // 画星形十边
                g.DrawLine(p, p1, p2);
                g.DrawLine(p, p2, p3);
                g.DrawLine(p, p3, p4);
                g.DrawLine(p, p4, p5);
                g.DrawLine(p, p5, p6);
                g.DrawLine(p, p6, p7);
                g.DrawLine(p, p7, p8);
                g.DrawLine(p, p8, p9);
                g.DrawLine(p, p9, p10);
                g.DrawLine(p, p10, p1);
                g.FillPolygon(b, points);// 填充图像

                // 更改显示区域
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(points);
                pictureBox2.Region = new Region(path);//设置星形的规格区域
                pictureBox2.Image = bitmap;
            }
        }

        // DIY: 添加圆形边框
        private void button17_Click(object sender, EventArgs e) {
            if (bitmap_after != null) {
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bitmap);// 得到Graphics对象

                int radius = 300;// 圆形框半径
                Pen p = new Pen(Color.Black, 2);//声明一个画笔,黑色，笔刷大小为2
                Brush b = new TextureBrush(bitmap_after);// 创建图像笔刷
                Rectangle r = new Rectangle(0, 0, radius, radius);//标识圆的大小
                g.DrawEllipse(p, r);// 画圆
                g.FillEllipse(b, r);// 填充图像

                // 更改显示区域
                GraphicsPath path = new GraphicsPath();
                path.AddEllipse(0, 0, radius, radius);
                pictureBox2.Region = new Region(path);//设置圆形的规格区域
                pictureBox2.Image = bitmap;
            }
        }

        // DIY：添加三角边框
        private void button16_Click(object sender, EventArgs e) {
            if (bitmap_after != null) {
                int width = bitmap_after.Width;
                int height = bitmap_after.Height;
                Bitmap bitmap = new Bitmap(width, height);
                Graphics g = Graphics.FromImage(bitmap);// 得到Graphics对象

                Pen p = new Pen(Color.Black, 2);//声明一个画笔,黑色，笔刷大小为2
                Brush b = new TextureBrush(bitmap_after);// 创建图像笔刷
                // 三角形的三个顶点
                Point p1 = new Point(150, 0);
                Point p2 = new Point(0, 300);
                Point p3 = new Point(300, 300);
                Point[] points = { p1, p2, p3 };
                // 画三角形三边
                g.DrawLine(p, p1, p2);
                g.DrawLine(p, p2, p3);
                g.DrawLine(p, p1, p3);
                g.FillPolygon(b, points);// 填充图像

                // 更改显示区域
                GraphicsPath path = new GraphicsPath();
                path.AddPolygon(points);
                pictureBox2.Region = new Region(path);// 设置三角形的规格区域
                pictureBox2.Image = bitmap;
            }
        }

    }
}
