﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Keys = System.Windows.Forms.Keys;

namespace KeyLogger
{
    internal struct KeyPress
    {
        private readonly Keys keyCode;
        private readonly bool isCapsLockOn;
        private readonly bool isShiftPressed;

        internal KeyPress(Keys keyCode, bool shiftPressed, bool capsLockOn)
        {
            this.keyCode = keyCode;
            this.isShiftPressed = shiftPressed;
            this.isCapsLockOn = capsLockOn;
        }        

        public override string ToString()
        {
            var character = ConvertKeysToString();

            if (IsAlphabeticKey())
            {
                //If both (shift and caps) are active then the string remains lowercase
                if (isShiftPressed == isCapsLockOn)
                    return character;
                else
                    return character.ToUpper();
            }
            else if (isShiftPressed)
            {
                return ShiftCharacterIfItIsShiftable(character);
            }

            return character;
        }

        private bool IsAlphabeticKey()
        {
            return (int)keyCode > 64 && (int)keyCode < 91;
        }

        private string ConvertKeysToString()
        {
            return this.keyCode switch
            {
                Keys.F1 => "<F1>",
                Keys.F2 => "<F2>",
                Keys.F3 => "<F3>",
                Keys.F4 => "<F4>",
                Keys.F5 => "<F5>",
                Keys.F6 => "<F6>",
                Keys.F7 => "<F7>",
                Keys.F8 => "<F8>",
                Keys.F9 => "<F9>",
                Keys.F10 => "<F10>",
                Keys.F11 => "<F11>",
                Keys.F12 => "<F12>",
                Keys.Snapshot => "<print screen>",
                Keys.Scroll => "<scroll>",
                Keys.Pause => "<pause>",
                Keys.Insert => "<insert>",
                Keys.Home => "<home>",
                Keys.Delete => "<delete>",
                Keys.End => "<end>",
                Keys.Prior => "<page up>",
                Keys.Next => "<page down>",
                Keys.Escape => "<esc>",
                Keys.NumLock => "<numlock>",
                Keys.Tab => "<tab>",
                Keys.Back => "<backspace>",
                Keys.Return => "<enter>",
                Keys.Space => "␣",
                Keys.Left => "<left>",
                Keys.Up => "<up>",
                Keys.Right => "<right>",
                Keys.Down => "<down>",

                Keys.LMenu | Keys.RMenu | Keys.Alt => "<alt>",
                Keys.LWin | Keys.RWin => "<win>",
                Keys.Capital => "<capsLock>",
                Keys.LControlKey | Keys.RControlKey => "<ctrl>",
                Keys.LShiftKey | Keys.RShiftKey => "<shift>",

                Keys.VolumeDown => "<volumeDown>",
                Keys.VolumeUp => "<volumeUp>",
                Keys.VolumeMute => "<volumeMute>",

                Keys.Multiply => "*",
                Keys.Add => "+",
                Keys.Separator => "|",
                Keys.Subtract => "-",
                Keys.Divide => "/",
                Keys.Oemplus => "=",
                Keys.Oemcomma => ",",
                Keys.OemMinus => "-",
                Keys.OemPeriod => ".",

                Keys.Decimal => ".",
                Keys.Oem1 => ";",
                Keys.Oem2 => "/",
                Keys.Oem3 => "`",
                Keys.Oem4 => "[",
                Keys.Oem5 => "\\",
                Keys.Oem6 => "]",
                Keys.Oem7 => "'",

                Keys.NumPad0 => "0",
                Keys.NumPad1 => "1",
                Keys.NumPad2 => "2",
                Keys.NumPad3 => "3",
                Keys.NumPad4 => "4",
                Keys.NumPad5 => "5",
                Keys.NumPad6 => "6",
                Keys.NumPad7 => "7",
                Keys.NumPad8 => "8",
                Keys.NumPad9 => "9",
                Keys.Q => "q",
                Keys.W => "w",
                Keys.E => "e",
                Keys.R => "r",
                Keys.T => "t",
                Keys.Y => "y",
                Keys.U => "u",
                Keys.I => "i",
                Keys.O => "o",
                Keys.P => "p",
                Keys.A => "a",
                Keys.S => "s",
                Keys.D => "d",
                Keys.F => "f",
                Keys.G => "g",
                Keys.H => "h",
                Keys.J => "j",
                Keys.K => "k",
                Keys.L => "l",
                Keys.Z => "z",
                Keys.X => "x",
                Keys.C => "c",
                Keys.V => "v",
                Keys.B => "b",
                Keys.N => "n",
                Keys.M => "m",
                Keys.D0 => "0",
                Keys.D1 => "1",
                Keys.D2 => "2",
                Keys.D3 => "3",
                Keys.D4 => "4",
                Keys.D5 => "5",
                Keys.D6 => "6",
                Keys.D7 => "7",
                Keys.D8 => "8",
                Keys.D9 => "9",

                _ => string.Empty,
            };
        }

        private string ShiftCharacterIfItIsShiftable(string character)
        {
            return this.keyCode switch
            {
                Keys.D1 => "!",
                Keys.D2 => "@",
                Keys.D3 => "#",
                Keys.D4 => "$",
                Keys.D5 => "%",
                Keys.D6 => "^",
                Keys.D7 => "&",
                Keys.D8 => "*",
                Keys.D9 => "(",
                Keys.D0 => ")",
                Keys.Oemcomma => "<",
                Keys.OemMinus => "_",
                Keys.OemPeriod => ">",
                Keys.Oemplus => "+",
                Keys.Oem1 => ":",
                Keys.Oem2 => "?",
                Keys.Oem3 => "~",
                Keys.Oem4 => "{",
                Keys.Oem5 => "|",
                Keys.Oem6 => "}",
                Keys.Oem7 => "\"",

                _ => character,
            };
        }
    }
}