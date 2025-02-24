using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BackEnd;
using LitJson;
using System;
using System.Linq;
using System.Reflection;


namespace Lance
{
    public class Coupon : AccountBase
    {
        List<CouponItem> mCouponList = new();

        public delegate void AfterGetCouponFunc(BackendReturnObject bro);

        public void UseCoupon(string couponCode, AfterGetCouponFunc onFinish)
        {
            //var couponItem = GetCoupon(couponName);
            //if (couponItem == null)
            //{
            //    UIUtil.ShowSystemErrorMessage("IsNotExistCoupon");

            //    return;
            //}

            SendQueue.Enqueue(Backend.Coupon.UseCoupon, couponCode, (bro) =>
            {
                onFinish?.Invoke(bro);
            });
        }

        CouponItem GetCoupon(string couponName)
        {
            return mCouponList.Where(x => x.Title == couponName).FirstOrDefault();
        }

        public void GetCouponList(AfterGetCouponFunc func)
        {
            bool isSuccess = false;
            //string className = GetType().Name;
            //string funcName = MethodBase.GetCurrentMethod()?.Name;
            //string errorInfo = string.Empty;

            //[뒤끝] 등록되어 있는 모든 쿠폰 긁어오자
            SendQueue.Enqueue(Backend.Coupon.CouponList, callback => {
                try
                {
                    if (callback.IsSuccess())
                    {
                        JsonData couponJson = callback.FlattenRows();

                        for (int i = 0; i < couponJson.Count; i++)
                        {
                            CouponItem coupon = new CouponItem();

                            coupon.Title = couponJson[i]["title"].ToString();
                            coupon.Uuid = couponJson[i]["uuid"].ToString();
                            coupon.Type = couponJson[i]["type"].ToString();
                            coupon.Version = couponJson[i]["version"].ToString();
                            coupon.Redundant = couponJson[i]["redundant"].ToString();

                            mCouponList.Add(coupon);
                        }
                    }

                    isSuccess = true;
                }
                catch (Exception e)
                {
                    //errorInfo = e.ToString();
                }
                finally
                {
                    func(callback);
                }
            });
        }
    }
}