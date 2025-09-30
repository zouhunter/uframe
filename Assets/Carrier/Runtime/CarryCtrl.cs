/*************************************************************************************//*
*  作者: 邹杭特                                                                       *
*  时间: 2021-05-02                                                                   *
*  版本: master_aeee4                                                                 *
*  功能:                                                                              *
*   - 载具控制器                                                                      *
*//************************************************************************************/

using System.Collections.Generic;

namespace UFrame.Carrier
{
    public class CarryCtrl
    {
        protected List<ICarrier> carriers;
        protected List<ICarrier> carriersCatch;//缓存需要继续处理的载具

        public CarryCtrl()
        {
            carriers = new List<ICarrier>();
            carriersCatch = new List<ICarrier>();
        }

        public void RegistCarrier(ICarrier carrier)
        {
            if (!carriersCatch.Contains(carrier))
            {
                if(!carrier.Started){
                    carrier.MoveStart();
                }
                carriersCatch.Add(carrier);
            }
        }

        public void OnUpdate()
        {
            if (carriersCatch.Count == 0 && carriers.Count == 0) return;

            carriers.Clear();
            carriers.AddRange(carriersCatch);
            carriersCatch.Clear();

            using (var enumerator = carriers.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    var carrier = enumerator.Current;
                   
                    if (carrier.JudgeArrived())
                    {
                        carrier.MoveComplete();
                    }
                    else
                    {
                        carrier.MoveFrame();
                        carriersCatch.Add(carrier);
                    }
                }
            }
        }

        public void OnRecover()
        {
            carriers.Clear();
            carriers.TrimExcess();
            carriersCatch.Clear();
            carriersCatch.TrimExcess();
        }
    }
}