//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-11-28 14:23:52
//* 描    述：  

//* ************************************************************************************
using UnityEngine;

namespace UFrame.Audio
{
    public class AudioSpectrumUtil
    {
        private static float[][] middleFrequenciesForBands = {
        new float[]{ 125.0f, 500, 1000, 2000 },
        new float[]{ 250.0f, 400, 600, 800 },
        new float[]{ 63.0f, 125, 500, 1000, 2000, 4000, 6000, 8000 },
        new float[]{ 31.5f, 63, 125, 250, 500, 1000, 2000, 4000, 8000, 16000 },
        new float[]{ 25.0f, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000 },
        new float[]{ 20.0f, 25, 31.5f, 40, 50, 63, 80, 100, 125, 160, 200, 250, 315, 400, 500, 630, 800, 1000, 1250, 1600, 2000, 2500, 3150, 4000, 5000, 6300, 8000, 10000, 12500, 16000, 20000 },
    };
        private static float[] bandwidthForBands = {
        1.414f, // 2^(1/2)
        1.260f, // 2^(1/3)
        1.414f, // 2^(1/2)
        1.414f, // 2^(1/2)
        1.122f, // 2^(1/6)
        1.122f  // 2^(1/6)
    };
        public static float[] GetMiddleFrequenciesForBand(AudioSpectrumBandType bandType)
        {
            return middleFrequenciesForBands[(int)bandType];
        }
        public static float GetBandwidthForBand(AudioSpectrumBandType bandType)
        {
            return bandwidthForBands[(int)bandType];
        }

        public static int FrequencyToSpectrumIndex(float f, int rawSpectrumLength)
        {
            var i = Mathf.FloorToInt(f / UnityEngine.AudioSettings.outputSampleRate * 2.0f * rawSpectrumLength);
            return Mathf.Clamp(i, 0, rawSpectrumLength - 1);
        }

        //take an array, and bin the values. 
        // if numBins is > intput.Length, duplicate input values
        // if numBins is < input.Length, average input values
        public static void GetBinnedArray(float[] input, float[] output)
        {
            if (output == null || input == null)
                return;

            var numBins = output.Length;
            if (numBins == input.Length)
                return;

            // if numBins is > intput.Length, duplicate input values
            if (numBins > input.Length)
            {
                int binsPerInput = numBins / input.Length + 1;
                //Debug.Log("BinsPerInput: " + binsPerInput);
                for (int b = 0; b < numBins; b++)
                {
                    int inputIndex = b / binsPerInput;
                    //Debug.Log( b + "%" + binsPerInput + "=" + inputIndex);
                    output[b] = input[inputIndex];
                }
            }

            // if numBins is < input.Length, downsample the input.
            if (numBins < input.Length)
            {
                //int inputsToSkip = input.Length - numBins;
                for (int b = 0; b < numBins; b++)
                {
                    output[b] = input[b];
                }
            }
        }

        //normalize array values to be in the range 0-1
        public static void NormalizeArray(float[] input)
        {
            float max = -Mathf.Infinity;
            //get the max value in the array
            for (int i = 0; i < input.Length; i++)
            {
                max = Mathf.Max(max, Mathf.Abs(input[i]));
            }

            //divide everything by the max value
            for (int i = 0; i < input.Length; i++)
            {
                input[i] = input[i] / max;
            }
        }

    }

    public enum AudioSpectrumBandType
    {
        FourBand,
        FourBandVisual,
        EightBand,
        TenBand,
        TwentySixBand,
        ThirtyOneBand
    }

    public enum AudioSpectrumClampType
    {
        Levels,
        PeakLevels,
        MeanLevels
    }
}