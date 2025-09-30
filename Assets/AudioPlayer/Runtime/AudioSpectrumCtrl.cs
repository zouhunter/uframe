//*************************************************************************************
//* 作    者： 邹杭特
//* 创建时间： 2021-11-28 14:12:11
//* 描    述： 音效控制

//* ************************************************************************************
using System.Collections.Generic;

using UnityEngine;

namespace UFrame.Audio
{
    [System.Serializable]
    public class AudioSpectrumCtrl
    {
        public float fallSpeed = 0.08f;
        public float sensibility = 8.0f;
        public AudioSpectrumBandType bandType = AudioSpectrumBandType.TenBand;
        public int numberOfSamples = 1024;

        private float[] rawSpectrum;
        private float[] levels;
        private float[] peakLevels;
        private float[] meanLevels;
        private int m_frameId;
        private AudioSpectrumBandType m_lastBandType;
        private int m_lastSamples;
        private float totalLevel;

        public float TotalLevel
        {
            get
            {
                if (Time.frameCount != m_frameId)
                {
                    m_frameId = Time.frameCount;
                    Refresh();
                }
                return totalLevel;
            }
        }

        public float[] Levels
        {
            get
            {
                if (Time.frameCount != m_frameId)
                {
                    m_frameId = Time.frameCount;
                    Refresh();
                }
                return levels;
            }
        }

        public float[] PeakLevels
        {
            get
            {
                if (Time.frameCount != m_frameId)
                {
                    m_frameId = Time.frameCount;
                    Refresh();
                }
                return peakLevels;
            }
        }

        public float[] MeanLevels
        {
            get
            {
                if (Time.frameCount != m_frameId)
                {
                    m_frameId = Time.frameCount;
                    Refresh();
                }
                return meanLevels;
            }
        }
        public AudioSpectrumCtrl()
        {
            ChangeBandType(bandType);
            ChangeSamples(numberOfSamples);
        }

        public AudioSpectrumCtrl(AudioSpectrumBandType bandType, int numberOfSamples = 1024)
        {
            this.bandType = bandType;
            ChangeBandType(bandType);
            ChangeSamples(numberOfSamples);
        }

        public float[] GetAudioSpectrumLevel(AudioSpectrumClampType clampType)
        {
            switch (clampType)
            {
                case AudioSpectrumClampType.Levels:
                    return Levels;
                case AudioSpectrumClampType.PeakLevels:
                    return PeakLevels;
                case AudioSpectrumClampType.MeanLevels:
                    return MeanLevels;
                default:
                    break;
            }
            return null;
        }

        public void ChangeBandType(AudioSpectrumBandType bandType)
        {
            var bandCount = AudioSpectrumUtil.GetMiddleFrequenciesForBand(bandType).Length;
            if (levels == null || levels.Length != bandCount)
            {
                levels = new float[bandCount];
                peakLevels = new float[bandCount];
                meanLevels = new float[bandCount];
            }
            m_lastBandType = bandType;
        }

        public void ChangeSamples(int numberOfSamples)
        {
            this.numberOfSamples = numberOfSamples;
            if (rawSpectrum == null || rawSpectrum.Length != numberOfSamples)
            {
                rawSpectrum = new float[numberOfSamples];
            }
            this.m_lastSamples = numberOfSamples;
        }

        private void Refresh()
        {
            if (m_lastBandType != bandType)
                ChangeBandType(bandType);
            if (m_lastSamples != numberOfSamples)
                ChangeSamples(numberOfSamples);

            AudioListener.GetSpectrumData(rawSpectrum, 0, FFTWindow.BlackmanHarris);
            float[] middlefrequencies = AudioSpectrumUtil.GetMiddleFrequenciesForBand(bandType);
            var bandwidth = AudioSpectrumUtil.GetBandwidthForBand(bandType);

            var falldown = fallSpeed * Time.deltaTime;
            var filter = Mathf.Exp(-sensibility * Time.deltaTime);
            var length = middlefrequencies.Length;
            totalLevel = 0;
            for (var bi = 0; bi < length; bi++)
            {
                int imin = AudioSpectrumUtil.FrequencyToSpectrumIndex(middlefrequencies[bi] / bandwidth, rawSpectrum.Length);
                int imax = AudioSpectrumUtil.FrequencyToSpectrumIndex(middlefrequencies[bi] * bandwidth, rawSpectrum.Length);

                var bandMax = 0.0f;
                for (var fi = imin; fi <= imax; fi++)
                {
                    bandMax = Mathf.Max(bandMax, rawSpectrum[fi]);
                }

                totalLevel += bandMax;
                levels[bi] = bandMax;
                peakLevels[bi] = Mathf.Max(peakLevels[bi] - falldown, bandMax);
                meanLevels[bi] = bandMax - (bandMax - meanLevels[bi]) * filter;
            }
        }
    }
}