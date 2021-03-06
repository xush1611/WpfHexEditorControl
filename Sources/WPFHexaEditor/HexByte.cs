﻿//////////////////////////////////////////////
// Apache 2.0  - 2016-2020
// Author : Derek Tremblay (derektremblay666@gmail.com)
// Contributor: Janus Tida
//////////////////////////////////////////////

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfHexaEditor.Core;
using WpfHexaEditor.Core.Bytes;
using WpfHexaEditor.Core.MethodExtention;

namespace WpfHexaEditor
{
    internal class HexByte : BaseByte
    {
        #region Global class variables

        private KeyDownLabel _keyDownLabel = KeyDownLabel.FirstChar;

        #endregion global class variables

        #region Constructor

        public HexByte(HexEditor parent) : base(parent)
        {
            //Update width
            UpdateDataVisualWidth();
        }

        #endregion Contructor

        #region Methods

        /// <summary>
        /// Update the render of text derived bytecontrol from byte property
        /// </summary>
        public override void UpdateTextRenderFromByte()
        {
            if (Byte != null)
            {
                Text = Byte.GetText(_parent.DataStringVisual, _parent.DataStringState, _parent.ByteOrder);
            }
            else
                Text = string.Empty;
        }

        public override void Clear()
        {
            base.Clear();
            _keyDownLabel = KeyDownLabel.FirstChar;
        }

        public void UpdateDataVisualWidth()
        {
            Width = CalculateCellWidth(_parent.ByteSize, _parent.DataStringVisual, _parent.DataStringState);
        }
        public static int CalculateCellWidth(ByteSizeType byteSize, DataVisualType type, DataVisualState state)
        { 
            var Width = byteSize switch
            {
                ByteSizeType.Bit8 => type switch
                {
                    DataVisualType.Decimal =>
                        state == DataVisualState.Changes ? 30 :
                        state == DataVisualState.ChangesPercent ? 35 : 25,
                    DataVisualType.Hexadecimal =>
                        state == DataVisualState.Changes ? 25 :
                        state == DataVisualState.ChangesPercent ? 35 : 20,
                    DataVisualType.Binary =>
                        state == DataVisualState.Changes ? 70 :
                        state == DataVisualState.ChangesPercent ? 65 : 65
                },
                ByteSizeType.Bit16 => type switch
                {
                    DataVisualType.Decimal =>
                        state == DataVisualState.Changes ? 30 :
                        state == DataVisualState.ChangesPercent ? 35 : 40,
                    DataVisualType.Hexadecimal =>
                        state == DataVisualState.Changes ? 40 :
                        state == DataVisualState.ChangesPercent ? 35 : 40,
                    DataVisualType.Binary =>
                        state == DataVisualState.Changes ? 70 :
                        state == DataVisualState.ChangesPercent ? 65 : 120
                },
                ByteSizeType.Bit32 => type switch
                {
                    DataVisualType.Decimal =>
                        state == DataVisualState.Changes ? 80 :
                        state == DataVisualState.ChangesPercent ? 35 : 80,
                    DataVisualType.Hexadecimal =>
                        state == DataVisualState.Changes ? 70 :
                        state == DataVisualState.ChangesPercent ? 35 : 70,
                    DataVisualType.Binary =>
                        state == DataVisualState.Changes ? 220 :
                        state == DataVisualState.ChangesPercent ? 65 : 220
                },
            };
            return Width;

        }

        #endregion Methods

            #region Events delegate

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && IsFocused)
            {
                //Is focused set editing to second char.
                _keyDownLabel = KeyDownLabel.SecondChar;
                UpdateCaret();
            }

            base.OnMouseDown(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (Byte == null) return;

            if (KeyValidation(e)) return;

            //MODIFY BYTE
            if (!ReadOnlyMode && KeyValidator.IsHexKey(e.Key))
            {
                if (_keyDownLabel == KeyDownLabel.NextPosition)
                {
                    _parent.AppendByte(new byte[] { 0 });
                    OnMoveNext(new EventArgs());
                }
                else
                {

                    bool isEndChar;
                    (Action, isEndChar) = Byte.Update(_parent.DataStringVisual, e.Key, ref _keyDownLabel);
                    if (isEndChar && _parent.Length != BytePositionInStream + 1)
                    {
                        _keyDownLabel = KeyDownLabel.NextPosition;
                        OnMoveNext(new EventArgs());
                    }
                }
                UpdateTextRenderFromByte();
            }
            UpdateCaret();

            base.OnKeyDown(e);
        }

        #endregion Events delegate

        #region Caret events/methods

        protected override void OnGotFocus(RoutedEventArgs e)
        {
            _keyDownLabel = KeyDownLabel.FirstChar;
            UpdateCaret();

            base.OnGotFocus(e);
        }

        private void UpdateCaret()
        {
            if (ReadOnlyMode || Byte == null)
                _parent.HideCaret();
            else
            {
                //TODO: clear size and use BaseByte.TextFormatted property...
                var size = Text[1].ToString()
                    .GetScreenSize(_parent.FontFamily, _parent.FontSize, _parent.FontStyle, FontWeight,
                        _parent.FontStretch, _parent.Foreground, this);

                _parent.SetCaretSize(Width / 2, size.Height);
                _parent.SetCaretMode(_parent.VisualCaretMode);

                switch (_keyDownLabel)
                {
                    case KeyDownLabel.FirstChar:
                        _parent.MoveCaret(TransformToAncestor(_parent).Transform(new Point(0, 0)));
                        break;
                    case KeyDownLabel.SecondChar:
                        _parent.MoveCaret(TransformToAncestor(_parent).Transform(new Point(size.Width + 1, 0)));
                        break;
                    case KeyDownLabel.NextPosition:
                        if (_parent.Length == BytePositionInStream + 1)
                            if (_parent.AllowExtend)
                            {
                                _parent.SetCaretMode(CaretMode.Insert);
                                _parent.MoveCaret(TransformToAncestor(_parent).Transform(new Point(size.Width * 2, 0)));
                            }
                            else
                                _parent.HideCaret();

                        break;
                }
            }
        }

        #endregion

    }
}
