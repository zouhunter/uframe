//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-11-28 15:57:38
//* 描    述： 时间音乐频率获取

//* ************************************************************************************
using System;

using UnityEngine;


namespace UFrame.Audio
{
    [Serializable]
    public class AudioFrequencyCtrl
    {
        public AudioSource source;
        public int numberOfSamples = 1024;
        public int freqMin = 20;//Hz
        public int freqMax = 60;//Hz

        private float[] rawSpectrum;
        private int fLow;
        private int fHigh;
        private int lastSample;
        private int lastFreqMin = 0;
        private int lastFreqMax = 0;

        public AudioFrequencyCtrl(AudioSource source)
        {
            this.source = source;
        }

        public AudioFrequencyCtrl()
        {
        }

        public void ChangeFreqMin(int min)
        {
            fLow = (int)Mathf.Floor(2 * min * numberOfSamples / AudioSettings.outputSampleRate);
            lastFreqMin = min;
        }

        public void ChangeFreqMax(int max)
        {
            fHigh = (int)Mathf.Floor(2 * max * numberOfSamples / AudioSettings.outputSampleRate);
            lastFreqMax = max;
        }

        public void ChangeSamples(int numberOfSamples)
        {
            this.numberOfSamples = numberOfSamples;
            if (rawSpectrum == null || rawSpectrum.Length != numberOfSamples)
            {
                rawSpectrum = new float[numberOfSamples];
            }
            lastSample = numberOfSamples;
        }

        public float GetFrequencyVol()
        {
            if (source && !source.mute)
            {
                CheckSampleRange();
                source.GetSpectrumData(rawSpectrum, 0, FFTWindow.BlackmanHarris);
                float sum = 0;
                for (int i = fLow; i <= fHigh; i++)
                {
                    if (i < rawSpectrum.Length && i >= 0)
                    {
                        sum += Mathf.Abs(rawSpectrum[i]);
                    }
                }
                sum = sum * source.volume;
                float frequency = sum / (fHigh - fLow + 1);
                return frequency;
            }
            return 0;
        }

        public float[] GetFrequencyData()
        {
            if (source && !source.mute)
            {
                CheckSampleRange();
                source.GetSpectrumData(rawSpectrum, 0, FFTWindow.BlackmanHarris);
                for (int i = fLow; i <= fHigh; i++)
                {
                    rawSpectrum[i] = rawSpectrum[i] * source.volume;
                }
                AudioSpectrumUtil.NormalizeArray(rawSpectrum);
                return rawSpectrum;
            }
            return null;
        }
        public bool GetFrequencyData(float[] output, bool abs = false)
        {
            if (output == null || output.Length == 0)
                return false;

            if (source && !source.mute)
            {
                CheckSampleRange();
                source.GetSpectrumData(rawSpectrum, 0, FFTWindow.BlackmanHarris);
                for (int i = fLow; i <= fHigh; i++)
                {
                    float frequency = rawSpectrum[i];
                    if (abs)
                        frequency = Mathf.Abs(rawSpectrum[i]);
                    frequency = frequency * source.volume;
                    rawSpectrum[i] = frequency;
                }
                AudioSpectrumUtil.GetBinnedArray(rawSpectrum, output);
                AudioSpectrumUtil.NormalizeArray(output);
                return true;
            }
            return false;
        }

        private void CheckSampleRange()
        {
            if (lastSample != numberOfSamples)
            {
                ChangeSamples(numberOfSamples);
            }
            if (lastFreqMin != freqMin)
            {
                ChangeFreqMin(freqMin);
            }
            if (lastFreqMax != freqMax)
            {
                ChangeFreqMax(freqMax);
            }
        }
    }
}