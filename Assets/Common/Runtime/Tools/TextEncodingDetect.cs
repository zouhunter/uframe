namespace UFrame
{
    public class TextEncodingDetect
    {
        private bool _nullSuggestsBinary = true;
        private double _utf16ExpectedNullPercent = 70;
        private double _utf16UnexpectedNullPercent = 10;

        public enum EncodingType
        {
            None, // Unknown or binary
            Ansi, // 0-255
            Ascii, // 0-127
            Utf7Bom,//Utf7 with Bom
            Utf8Bom, // UTF8 with BOM
            Utf8Nobom, // UTF8 without BOM
            Utf16LeBom, // UTF16 LE with BOM
            Utf16LeNoBom, // UTF16 LE without BOM
            Utf16BeBom, // UTF16-BE with BOM
            Utf16BeNoBom, // UTF16-BE without BOM
            Utf32Bom,
        }

        /// <summary>
        ///     Sets if the presence of nulls in a buffer indicate the buffer is binary data rather than text.
        /// </summary>
        public bool NullSuggestsBinary
        {
            set
            {
                _nullSuggestsBinary = value;
            }
        }

        public double Utf16ExpectedNullPercent
        {
            set
            {
                if (value > 0 && value < 100)
                {
                    _utf16ExpectedNullPercent = value;
                }
            }
        }

        public double Utf16UnexpectedNullPercent
        {
            set
            {
                if (value > 0 && value < 100)
                {
                    _utf16UnexpectedNullPercent = value;
                }
            }
        }

        /// <summary>
        ///     Gets the BOM length for a given Encoding mode.
        /// </summary>
        /// <param name="encoding"></param>
        /// <returns>The BOM length.</returns>
        public static int GetBomLengthFromEncodingMode(EncodingType encoding)
        {
            int length;

            switch (encoding)
            {
                case EncodingType.Utf16BeBom:
                case EncodingType.Utf16LeBom:
                    length = 2;
                    break;

                case EncodingType.Utf8Bom:
                    length = 3;
                    break;

                default:
                    length = 0;
                    break;
            }

            return length;
        }

        /// <summary>
        ///     Checks for a BOM sequence in a byte buffer.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="size"></param>
        /// <returns>Encoding type or Encoding.None if no BOM.</returns>
        public EncodingType CheckBom(byte[] buffer, int size)
        {
            // Read the BOM
            var b0 = buffer.Length > 0 ? buffer[0] : 0;
            var b1 = buffer.Length > 1 ? buffer[1] : 0;
            var b2 = buffer.Length > 2 ? buffer[2] : 0;
            var b3 = buffer.Length > 3 ? buffer[3] : 0;
            // Analyze the BOM
            if (b0 == 0x2b && b1 == 0x2f && b2 == 0x76) return EncodingType.Utf7Bom;
            if (b0 == 0xef && b1 == 0xbb && b2 == 0xbf) return EncodingType.Utf8Bom;
            if (b0 == 0xff && b1 == 0xfe) return EncodingType.Utf16LeBom; //UTF-16LE
            if (b0 == 0xfe && b1 == 0xff) return EncodingType.Utf16BeBom; //UTF-16BE
            if (b0 == 0 && b1 == 0 && b2 == 0xfe && b3 == 0xff) return EncodingType.Utf32Bom;
            return EncodingType.None;
        }

        public System.Text.Encoding DetectEncoding(byte[] buffer, int size)
        {
            var encodingType = DetectEncodingType(buffer, size);
            switch (encodingType)
            {
                case TextEncodingDetect.EncodingType.Ansi: return System.Text.Encoding.GetEncoding("gb2312");
                case TextEncodingDetect.EncodingType.Ascii: return System.Text.Encoding.ASCII;
                case TextEncodingDetect.EncodingType.Utf7Bom:
                    break;
                case TextEncodingDetect.EncodingType.Utf8Bom:
                    return System.Text.Encoding.UTF8;
                case TextEncodingDetect.EncodingType.Utf8Nobom:
                    return System.Text.Encoding.GetEncoding("utf-8");
                case TextEncodingDetect.EncodingType.Utf16LeBom:
                    return System.Text.Encoding.Unicode;
                case TextEncodingDetect.EncodingType.Utf16LeNoBom:
                    break;
                case TextEncodingDetect.EncodingType.Utf16BeBom:
                    break;
                case TextEncodingDetect.EncodingType.Utf16BeNoBom:
                    break;
                case TextEncodingDetect.EncodingType.Utf32Bom:
                    break;
                default:
                    break;
            }
            return null;
        }

        /// <summary>
        ///     Automatically detects the Encoding type of a given byte buffer.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>The Encoding type or Encoding.None if unknown.</returns>
        public EncodingType DetectEncodingType(byte[] buffer, int size)
        {
            // First check if we have a BOM and return that if so
            EncodingType encoding = CheckBom(buffer, size);
            if (encoding != EncodingType.None)
            {
                return encoding;
            }

            // Now check for valid UTF8
            encoding = CheckUtf8(buffer, size);
            if (encoding != EncodingType.None)
            {
                return encoding;
            }

            // Now try UTF16 
            encoding = CheckUtf16NewlineChars(buffer, size);
            if (encoding != EncodingType.None)
            {
                return encoding;
            }

            encoding = CheckUtf16Ascii(buffer, size);
            if (encoding != EncodingType.None)
            {
                return encoding;
            }

            // ANSI or None (binary) then
            if (!DoesContainNulls(buffer, size))
            {
                return EncodingType.Ansi;
            }

            // Found a null, return based on the preference in null_suggests_binary_
            return _nullSuggestsBinary ? EncodingType.None : EncodingType.Ansi;
        }

        /// <summary>
        ///     Checks if a buffer contains text that looks like utf16 by scanning for
        ///     newline chars that would be present even in non-english text.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>Encoding.none, Encoding.Utf16LeNoBom or Encoding.Utf16BeNoBom.</returns>
        private static EncodingType CheckUtf16NewlineChars(byte[] buffer, int size)
        {
            if (size < 2)
            {
                return EncodingType.None;
            }

            // Reduce size by 1 so we don't need to worry about bounds checking for pairs of bytes
            size--;

            var leControlChars = 0;
            var beControlChars = 0;

            uint pos = 0;
            while (pos < size)
            {
                byte ch1 = buffer[pos++];
                byte ch2 = buffer[pos++];

                if (ch1 == 0)
                {
                    if (ch2 == 0x0a || ch2 == 0x0d)
                    {
                        ++beControlChars;
                    }
                }
                else if (ch2 == 0)
                {
                    if (ch1 == 0x0a || ch1 == 0x0d)
                    {
                        ++leControlChars;
                    }
                }

                // If we are getting both LE and BE control chars then this file is not utf16
                if (leControlChars > 0 && beControlChars > 0)
                {
                    return EncodingType.None;
                }
            }

            if (leControlChars > 0)
            {
                return EncodingType.Utf16LeNoBom;
            }

            return beControlChars > 0 ? EncodingType.Utf16BeNoBom : EncodingType.None;
        }

        /// <summary>
        /// Checks if a buffer contains any nulls. Used to check for binary vs text data.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        private static bool DoesContainNulls(byte[] buffer, int size)
        {
            uint pos = 0;
            while (pos < size)
            {
                if (buffer[pos++] == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Checks if a buffer contains text that looks like utf16. This is done based
        ///     on the use of nulls which in ASCII/script like text can be useful to identify.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>Encoding.none, Encoding.Utf16LeNoBom or Encoding.Utf16BeNoBom.</returns>
        private EncodingType CheckUtf16Ascii(byte[] buffer, int size)
        {
            var numOddNulls = 0;
            var numEvenNulls = 0;

            // Get even nulls
            uint pos = 0;
            while (pos < size)
            {
                if (buffer[pos] == 0)
                {
                    numEvenNulls++;
                }

                pos += 2;
            }

            // Get odd nulls
            pos = 1;
            while (pos < size)
            {
                if (buffer[pos] == 0)
                {
                    numOddNulls++;
                }

                pos += 2;
            }

            double evenNullThreshold = numEvenNulls * 2.0 / size;
            double oddNullThreshold = numOddNulls * 2.0 / size;
            double expectedNullThreshold = _utf16ExpectedNullPercent / 100.0;
            double unexpectedNullThreshold = _utf16UnexpectedNullPercent / 100.0;

            // Lots of odd nulls, low number of even nulls
            if (evenNullThreshold < unexpectedNullThreshold && oddNullThreshold > expectedNullThreshold)
            {
                return EncodingType.Utf16LeNoBom;
            }

            // Lots of even nulls, low number of odd nulls
            if (oddNullThreshold < unexpectedNullThreshold && evenNullThreshold > expectedNullThreshold)
            {
                return EncodingType.Utf16BeNoBom;
            }

            // Don't know
            return EncodingType.None;
        }

        /// <summary>
        ///     Checks if a buffer contains valid utf8.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="size">The size of the byte buffer.</param>
        /// <returns>
        ///     Encoding type of Encoding.None (invalid UTF8), Encoding.Utf8NoBom (valid utf8 multibyte strings) or
        ///     Encoding.ASCII (data in 0.127 range).
        /// </returns>
        /// <returns>2</returns>
        private EncodingType CheckUtf8(byte[] buffer, int size)
        {
            // UTF8 Valid sequences
            // 0xxxxxxx  ASCII
            // 110xxxxx 10xxxxxx  2-byte
            // 1110xxxx 10xxxxxx 10xxxxxx  3-byte
            // 11110xxx 10xxxxxx 10xxxxxx 10xxxxxx  4-byte
            //
            // Width in UTF8
            // Decimal      Width
            // 0-127        1 byte
            // 194-223      2 bytes
            // 224-239      3 bytes
            // 240-244      4 bytes
            //
            // Subsequent chars are in the range 128-191
            var onlySawAsciiRange = true;
            uint pos = 0;

            while (pos < size)
            {
                byte ch = buffer[pos++];

                if (ch == 0 && _nullSuggestsBinary)
                {
                    return EncodingType.None;
                }

                int moreChars;
                if (ch <= 127)
                {
                    // 1 byte
                    moreChars = 0;
                }
                else if (ch >= 194 && ch <= 223)
                {
                    // 2 Byte
                    moreChars = 1;
                }
                else if (ch >= 224 && ch <= 239)
                {
                    // 3 Byte
                    moreChars = 2;
                }
                else if (ch >= 240 && ch <= 244)
                {
                    // 4 Byte
                    moreChars = 3;
                }
                else
                {
                    return EncodingType.None; // Not utf8
                }

                // Check secondary chars are in range if we are expecting any
                while (moreChars > 0 && pos < size)
                {
                    onlySawAsciiRange = false; // Seen non-ascii chars now

                    ch = buffer[pos++];
                    if (ch < 128 || ch > 191)
                    {
                        return EncodingType.None; // Not utf8
                    }

                    --moreChars;
                }
            }

            // If we get to here then only valid UTF-8 sequences have been processed

            // If we only saw chars in the range 0-127 then we can't assume UTF8 (the caller will need to decide)
            return onlySawAsciiRange ? EncodingType.Ascii : EncodingType.Utf8Nobom;
        }
    }
}