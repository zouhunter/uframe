using UnityEngine;
using System.Collections;

namespace UFrame.Audio
{
    public class AudioSpectrumBehaviour : MonoBehaviour
    {
        #region Public variables
        public int numberOfSamples = 1024;
        public AudioSpectrumBandType bandType = AudioSpectrumBandType.TenBand;
        public float fallSpeed = 0.08f;
        public float sensibility = 8.0f;
        #endregion

        #region Private variables
        float[] rawSpectrum;
        float[] levels;
        float[] peakLevels;
        float[] meanLevels;
        #endregion

        #region Public property
        public float[] Levels
        {
            get { return levels; }
        }

        public float[] PeakLevels
        {
            get { return peakLevels; }
        }

        public float[] MeanLevels
        {
            get { return meanLevels; }
        }
        #endregion

        #region Private functions
        void CheckBuffers()
        {
            if (rawSpectrum == null || rawSpectrum.Length != numberOfSamples)
            {
                rawSpectrum = new float[numberOfSamples];
            }
            var bandCount = AudioSpectrumUtil.GetMiddleFrequenciesForBand(bandType).Length;
            if (levels == null || levels.Length != bandCount)
            {
                levels = new float[bandCount];
                peakLevels = new float[bandCount];
                meanLevels = new float[bandCount];
            }
        }

        #endregion

        #region Monobehaviour functions
        void Awake()
        {
            CheckBuffers();
        }

        void Update()
        {
            CheckBuffers();

            AudioListener.GetSpectrumData(rawSpectrum, 0, FFTWindow.BlackmanHarris);

            float[] middlefrequencies = AudioSpectrumUtil.GetMiddleFrequenciesForBand(bandType);
            var bandwidth = AudioSpectrumUtil.GetBandwidthForBand(bandType);

            var falldown = fallSpeed * Time.deltaTime;
            var filter = Mathf.Exp(-sensibility * Time.deltaTime);

            for (var bi = 0; bi < levels.Length; bi++)
            {
                int imin = AudioSpectrumUtil.FrequencyToSpectrumIndex(middlefrequencies[bi] / bandwidth, rawSpectrum.Length);
                int imax = AudioSpectrumUtil.FrequencyToSpectrumIndex(middlefrequencies[bi] * bandwidth, rawSpectrum.Length);

                var bandMax = 0.0f;
                for (var fi = imin; fi <= imax; fi++)
                {
                    bandMax = Mathf.Max(bandMax, rawSpectrum[fi]);
                }

                levels[bi] = bandMax;
                peakLevels[bi] = Mathf.Max(peakLevels[bi] - falldown, bandMax);
                meanLevels[bi] = bandMax - (bandMax - meanLevels[bi]) * filter;
            }
        }
        #endregion
    }
}