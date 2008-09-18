using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace MarkerMaker
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public enum MarkerType
        {
            BchThin,
            SimpleStd,
            SimpleThin,
        }

        MarkerType _markerType;
        int _origMarkerSize;
        string _markerNameStart;
        string _markerNumFormat;

        private void Form1_Load(object sender, EventArgs e)
        {
            cbMarkerType.Items.Add("BchThin");
            cbMarkerType.Items.Add("SimpleStd");
            cbMarkerType.Items.Add("SimpleThin");
            cbMarkerType.SelectedIndex = 0;
            
            txtStartMarker.Text = "480";
            txtEndMarker.Text = "499";
            txtNumRows.Text = "4";
            txtNumCols.Text = "5";
            txtLeftMargin.Text = "10";
            txtTopMargin.Text = "10";
            txtMultiplier.Text = "5";
        }

        private void cbMarkerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //BchThin_0000.png 8x8
            //SimpleStd_000.png 12x12
            //SimpleThin_000.png 8x8

            _markerType = (MarkerType) cbMarkerType.SelectedIndex;

            switch (_markerType)
            {
                case MarkerType.BchThin:
                    txtIdPath.Text = @"..\..\id-markers\bch";
                    _origMarkerSize = 8;
                    _markerNameStart = "BchThin_";
                    _markerNumFormat = "0000";
                    break;
                case MarkerType.SimpleStd:
                    txtIdPath.Text = @"..\..\id-markers\simple\std-border";
                    _origMarkerSize = 12;
                    _markerNameStart = "SimpleStd_";
                    _markerNumFormat = "000";
                    break;
                case MarkerType.SimpleThin:
                    txtIdPath.Text = @"..\..\id-markers\simple\thin-border";
                    _origMarkerSize = 8;
                    _markerNameStart = "SimpleThin_";
                    _markerNumFormat = "000";
                    break;
            }
        }

        #region UI_PROPS
        public int GetStartMarker()
        {
            return Int32.Parse(txtStartMarker.Text.Trim());
        }

        public int GetEndMarker()
        {
            return Int32.Parse(txtEndMarker.Text.Trim());
        }

        public int GetNumRows()
        {
            return Int32.Parse(txtNumRows.Text.Trim());
        }

        public int GetNumCols()
        {
            return Int32.Parse(txtNumCols.Text.Trim());
        }

        public int GetLeftMargin()
        {
            return Int32.Parse(txtLeftMargin.Text.Trim());
        }

        public int GetTopMargin()
        {
            return Int32.Parse(txtTopMargin.Text.Trim());
        }

        public int GetMultiplier()
        {
            return Int32.Parse(txtMultiplier.Text.Trim());
        }
        #endregion

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            try
            {
                string markerPath = txtIdPath.Text.Trim();
                DirectoryInfo diMarkers = new DirectoryInfo(markerPath);

                int markerSize = _origMarkerSize * GetMultiplier();
                int totalWidth = ((GetLeftMargin() + markerSize) * GetNumCols()) - GetLeftMargin();
                int totalHeight = ((GetTopMargin() + markerSize) * GetNumRows()) - GetTopMargin();
                Bitmap bOut = new Bitmap(totalWidth, totalHeight);
                Graphics gOut = Graphics.FromImage(bOut);
                gOut.Clear(Color.White);
                gOut.Dispose();

                for (int row = 0; row < GetNumRows(); row++)
                {
                    for (int col = 0; col < GetNumCols(); col++)
                    {
                        int markerOffset = (row * GetNumCols()) + col;
                        int currentMarker = GetStartMarker() + markerOffset;

                        if (currentMarker > GetEndMarker())
                        {
                            continue;
                        }

                        int markerLeft = ((GetLeftMargin() + markerSize) * col);
                        int markerTop = ((GetTopMargin() + markerSize) * row);
                        Bitmap bMarker = GetMarker(diMarkers, currentMarker);
                        for (int yMark = 0; yMark < bMarker.Height; yMark++)
                        {
                            for (int xMark = 0; xMark < bMarker.Width; xMark++)
                            {
                                Color c = bMarker.GetPixel(xMark, yMark);
                                int xCell = markerLeft + xMark * GetMultiplier();
                                int yCell = markerTop + yMark * GetMultiplier();
                                for (int x = xCell; x < xCell + GetMultiplier(); x++)
                                {
                                    for (int y = yCell; y < yCell + GetMultiplier(); y++)
                                    {
                                        bOut.SetPixel(x, y, c);
                                    }
                                }
                            }
                        }
                        bMarker.Dispose();
                    }
                }

                bOut.Save("markers.png", System.Drawing.Imaging.ImageFormat.Png);
                //gOut.Dispose();
                bOut.Dispose();
                MessageBox.Show("done");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private Bitmap GetMarker(DirectoryInfo diMarkers, int markerId)
        {
            string markerName = _markerNameStart + markerId.ToString(_markerNumFormat) + ".png"; //BCH
            //string markerName = "SimpleThin_" + markerId.ToString("000") + ".png"; //simple
            string markerPath = diMarkers.FullName + @"\" + markerName;
            Bitmap b = (Bitmap) Bitmap.FromFile(markerPath);
            return b;
        }


    }
}