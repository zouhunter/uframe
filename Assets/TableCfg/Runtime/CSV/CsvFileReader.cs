/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-04-18                                                                   *
*  版本: master_482d3f                                                                *
*  功能:                                                                              *
*   - csv表文件序列化                                                                 *
*//************************************************************************************/
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;

namespace UFrame.TableCfg
{
    public class CsvFileReader : System.IO.BinaryReader
    {
        private char[] charGroup;
        private int charIndex;
        private int charEnd;
        private int CHAR_GROUP_HALF = 512;
        private long streamNum;
        private Func<StringBuilder> getSbFunc;

        public CsvFileReader(Stream stream, Encoding encoding, System.Func<StringBuilder> getSbFunc)
            : base(stream, encoding)
        {
            this.getSbFunc = getSbFunc;
            streamNum = stream.Length;
            charGroup = new char[CHAR_GROUP_HALF * 2];
        }

        private void FillCharGroup()
        {
            if (streamNum <= BaseStream.Position)
                return;

            if (charEnd == 0 || charIndex > CHAR_GROUP_HALF)
            {
                if (charEnd > charIndex)
                {
                    for (int i = charIndex; i < charEnd; i++)
                    {
                        charGroup[i - charIndex] = charGroup[i];
                    }
                }
                charEnd = charEnd - charIndex;
                var maxReadNum = CHAR_GROUP_HALF * 2 - charEnd;
                var readNum = Read(charGroup, charEnd, maxReadNum);
                charEnd += readNum;
                charIndex = 0;
            }
        }


        /// <summary>  
        /// Reads a row of data from a CSV file  
        /// </summary>  
        /// <param name="row"></param>  
        /// <returns></returns>  
        public int ReadRow(List<StringBuilder> row)
        {
            foreach (var element in row)
                element.Clear();

            int qud = 0;
            var lineFinish = false;
            int rowIndex = 0;
            while (!lineFinish)
            {
                FillCharGroup();
                if (charIndex >= charEnd)
                {
                    if(rowIndex > 0 && !lineFinish)
                    {
                        rowIndex++;
                    }
                    break;
                }

                int startIndex = -1;
                int endIndex = -1;
                while (charIndex < charEnd && !lineFinish)
                {
                    var currentChar = charGroup[charIndex];
                    var wordFinish = false;
                    switch (currentChar)
                    {
                        case '"':
                            qud++;
                            if (startIndex < 0)
                                startIndex = charIndex + 1;
                            else if(qud % 2 != 0)
                                endIndex = charIndex;
                            break;
                        case '\n':
                            if (qud % 2 == 0)
                                lineFinish = true;
                            else
                                endIndex = charIndex;
                            break;
                        case ',':
                            if (qud % 2 == 0)
                                wordFinish = true;
                            break;
                        default:
                            if (startIndex < 0)
                                startIndex = charIndex;
                            endIndex = charIndex+1;
                            break;
                    }
                    charIndex++;
                    if (wordFinish || lineFinish || charIndex >= charEnd)
                    {
                        StringBuilder sb;
                        if (rowIndex < row.Count)
                        {
                            sb = row[rowIndex];
                        }
                        else
                        {
                            sb = this.getSbFunc.Invoke();
                            row.Add(sb);
                        }
                        if (startIndex >= 0)
                        {
                            var textLength = endIndex - startIndex;
                            if (textLength > 0)
                            {
                                sb.Append(charGroup, startIndex, textLength);
                            }
                            if(qud > 2)
                            {
                                sb.Replace("\"\"", "\"");
                            }
                            startIndex = -1;
                        }
                        if (lineFinish || wordFinish)
                        {
                            rowIndex++;
                            qud = 0;
                        }
                    }
                }
            }
            return rowIndex;
        }
    }
}
