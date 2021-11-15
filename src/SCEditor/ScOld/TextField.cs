﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCEditor.ScOld
{
    class TextField : ScObject
    {
        private string _fontName;
        private ushort _unk16; // textFieldBounds 1 > float
        private ushort _unk18; // textFieldBounds 2 > float
        private ushort _unk20; // textFieldBounds 3 > float
        private ushort _unk22; // textFieldBounds 4 > float
        private int _unk24; // numbertext
        private int _unk28; // numberValue
        private string _unk32; // stringObject - text
        private byte _flag;
        private byte _unk42; // align
        private byte _unk43; // characterScale - fontSize
        private ushort _unk44;
        private ushort _unk46;
        private ScFile _scFile;
        private byte _dataType;
        private bool _flag1;
        private bool _flag2;
        private bool _flag4;
        private bool _flag8;
        private bool _flag16; // multiline
        private bool _flag64;
        private bool _unkR1;
        private ushort _unkR2;
        private ushort _unk46tmp;
        // 124/248 interastiveResursive bool
        // 228 and 232 highlightRange
        // this + 84 == matrix

        public string fontName => _fontName;

        public TextField(ScFile scFile)
        {
            _scFile = scFile;
        }

        public TextField(ScFile scFile, byte dataType)
        {
            _scFile = scFile;
            _dataType = dataType;
        }

        public override int GetDataType()
        {
            return 5;
        }

        public override string GetDataTypeName()
        {
            return "TextFields";
        }

        public override void Read(BinaryReader br, string packetid)
        {
            Id = br.ReadUInt16(); // 82

            byte stringLength = br.ReadByte();
            if (stringLength < 255)
            {
                _fontName = Encoding.ASCII.GetString(br.ReadBytes(stringLength)); // 128
            }

            if (!string.IsNullOrEmpty(_fontName))
            {
                if (_scFile.getFontNames().FindIndex(n => n == _fontName) == -1)
                    _scFile.addFontName(_fontName);
            }

            _unk24 = br.ReadInt32(); // 112 224

            if (br.ReadBoolean())
            {
                _flag |= 4; // 153
                _flag4 = true;
            } 
            if (br.ReadBoolean())
            {
                _flag |= 8; // 154
                _flag8 = true;
            } 
            if (br.ReadBoolean())
            {
                _flag |= 16; // 155
                _flag16 = true;
            }

            _unkR1 = br.ReadBoolean(); // 156
            _unk42 = br.ReadByte(); // 168 42
            _unk43 = br.ReadByte(); // 172 43
            _unk16 = br.ReadUInt16(); // 176 44 
            _unk18 = br.ReadUInt16(); // 180 45
            _unk20 = br.ReadUInt16(); // 184 46
            _unk22 = br.ReadUInt16(); // 188 47

            if (br.ReadBoolean())
            {
                _flag |= 2; // 152
                _flag2 = true;
            }
                
            byte stringLength2 = br.ReadByte();
            if (stringLength2 < 255)
            {
                _unk32 = Encoding.ASCII.GetString(br.ReadBytes(stringLength2)); // 196
            }

            if (packetid == "07")
                return;

            if (br.ReadBoolean())
            {
                _flag |= 1;
                _flag1 = true;
            }
                
            switch (packetid)
            {
                case "14":
                    _flag |= 32;
                    break;
                case "15":
                    _flag |= 32;
                    _unk28 = br.ReadInt32();
                    break;
                case "19":
                    _unk28 = br.ReadInt32();
                    break;

                case "21":
                case "2B":
                case "2C":
                    _unk28 = br.ReadInt32();
                    _unk44 = br.ReadUInt16();
                    _unkR2 = br.ReadUInt16();
                    _flag |= 32;

                    if (packetid == "2B" || packetid == "2C")
                    {
                        _unk46tmp = br.ReadUInt16();
                        _unk46 = (ushort)(((uint)(0x7FFF * _unk46tmp + ((-40656255836343L * _unk46tmp) >> 32)) >> 31) + ((uint)(0x7FFF * _unk46tmp + ((-40656255836343L * _unk46tmp) >> 32)) >> 8));
                    }

                    if (packetid == "2C" && br.ReadBoolean())
                    {
                        _flag |= 64;
                        _flag64 = true;
                    }    

                    break;
            }
        }

        public override void Write(FileStream input)
        {
            if (customAdded)
            {
                input.Seek(_scFile.GetEofOffset(), SeekOrigin.Begin);

                // DataType and Length
                input.WriteByte(_dataType);
                input.Write(BitConverter.GetBytes(0), 0, 4);

                int dataLength = 0;

                // ID
                input.Write(BitConverter.GetBytes(Id), 0, 2);
                dataLength += 2;

                // Font Name
                if (string.IsNullOrEmpty(_fontName))
                {
                    input.Write(BitConverter.GetBytes(0xFF));
                }
                else
                {
                    byte[] stringData = Encoding.ASCII.GetBytes(_fontName);
                    input.WriteByte(BitConverter.GetBytes(stringData.Length)[0]);
                    input.Write(stringData, 0, stringData.Length);

                    dataLength += stringData.Length;
                }
                dataLength += 1;

                // REST OF DATA
                input.Write(BitConverter.GetBytes(_unk24), 0, 4);
                dataLength += 4;

                input.Write(BitConverter.GetBytes(_flag4), 0, 1);
                input.Write(BitConverter.GetBytes(_flag8), 0, 1);
                input.Write(BitConverter.GetBytes(_flag16), 0, 1);
                dataLength += 3;

                input.Write(BitConverter.GetBytes(_unkR1), 0, 1);
                input.WriteByte(_unk42);
                input.WriteByte(_unk43);
                input.Write(BitConverter.GetBytes(_unk16), 0, 2);
                input.Write(BitConverter.GetBytes(_unk18), 0, 2);
                input.Write(BitConverter.GetBytes(_unk20), 0, 2);
                input.Write(BitConverter.GetBytes(_unk22), 0, 2);
                dataLength += 11;

                input.Write(BitConverter.GetBytes(_flag2), 0, 1);
                dataLength += 1;

                // unk32
                if (string.IsNullOrEmpty(_unk32))
                {
                    input.Write(BitConverter.GetBytes(0xFF));
                }
                else
                {
                    byte[] stringData = Encoding.ASCII.GetBytes(_unk32);
                    input.WriteByte(BitConverter.GetBytes(stringData.Length)[0]);
                    input.Write(stringData, 0, stringData.Length);

                    dataLength += stringData.Length;
                }
                dataLength += 1;

                if (_dataType != 7)
                {
                    input.Write(BitConverter.GetBytes(_flag1), 0, 1);
                    dataLength += 1;

                    switch(_dataType)
                    {
                        case 20:
                            // flag32
                            break;

                        case 21:
                        case 25:
                            input.Write(BitConverter.GetBytes(_unk28), 0, 4);
                            dataLength += 4;
                            break;

                        case 33:
                        case 43:
                        case 44:
                            input.Write(BitConverter.GetBytes(_unk28), 0, 4);
                            input.Write(BitConverter.GetBytes(_unk44), 0, 2);
                            input.Write(BitConverter.GetBytes(_unkR2), 0, 2);
                            dataLength += 8;

                            if (_dataType == 43 || _dataType == 44)
                            {
                                input.Write(BitConverter.GetBytes(_unk46tmp), 0, 2);
                                dataLength += 2;
                            }

                            if (_dataType == 44)
                            {
                                input.Write(BitConverter.GetBytes(_flag64), 0, 1);
                                dataLength += 1;
                            }
                            break;

                    }
                }

                input.Seek(-(dataLength + 4), SeekOrigin.Current);
                input.Write(BitConverter.GetBytes(dataLength + 5), 0, 4);
                input.Seek(dataLength, SeekOrigin.Current);
            }

            if (_offset < 0 || customAdded == true)
            {
                input.Write(new byte[] { 0, 0, 0, 0, 0 }, 0, 5);
                _offset = _scFile.GetEofOffset();
            }
        }

        public override Bitmap Render(RenderingOptions options)
        {
            // Matrix matrixInput = new Matrix(); // multiply = this + 20, input matrix a2
            // Color colorTransform = new Color(); // multipy = _unk16, colortransform, _unk28 | this + 27 | input int a4

            // (this + 40) = (this + 40) + input float a5 % 1.0

            // shape = testfield.shapeRender(this, matrixInput, _unk24, colorTransform)

            // if shape == 1 > this + 20 = colorTransform & 0xFFFFFF00

            return shapeRender(); // shape
        }

        private Bitmap shapeRender()
        {

            return null;

            //Bitmap image = new Bitmap(120, 120);
            //RectangleF rectf = new RectangleF(0, 0, image.Width, image.Height);

            //Graphics g = Graphics.FromImage(image);
            //g.SmoothingMode = SmoothingMode.AntiAlias;
            //g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            //g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            //g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            //StringFormat format = new StringFormat()
            //{
            //    Alignment = StringAlignment.Center,
            //    LineAlignment = StringAlignment.Center
            //};

            //g.DrawString("yourText", new Font("Tahoma", 8), Brushes.Black, rectf, format);
            //g.Flush();

            //return image;
        }

        public void setId(ushort id)
        {
            this.Id = id;
        }
    }
}
