using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using UnityEngine.Purchasing.Extension;
using System;
using BackEnd;

namespace Lance
{
    class IAPManager : IDetailedStoreListener
    {
        IStoreController mStoreController; //구매 과정을 제어하는 함수 제공자
        IExtensionProvider mStoreExtensionProvider; //여러 플랫폼을 위한 확장 처리 제공자

        Action mOnPurchased;

        public void Init()
        {
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            /* 구글 플레이 상품들 추가 */
            // 잼 상품
            foreach (var gemShopData in Lance.GameData.GemShopData.Values)
            {
                if (gemShopData.productId.IsValid())
                {
                    builder.AddProduct(gemShopData.productId, ProductType.Consumable, new IDs() { { gemShopData.productId, GooglePlay.Name } });
                }
            }

            // 패키지 상품
            foreach (var packageShopData in Lance.GameData.PackageShopData)
            {
                if (packageShopData.productId.IsValid())
                {
                    ProductType type = ProductType.NonConsumable;

                    if (packageShopData.resetType != PackageResetType.None)
                    {
                        type = ProductType.Consumable;
                    }
                    else if (packageShopData.resetType == PackageResetType.None)
                    {
                        if (packageShopData.purchaseLimitCount > 1)
                        {
                            type = ProductType.Consumable;
                        }
                        else
                        {
                            type = ProductType.NonConsumable;
                        }
                    }

                    builder.AddProduct(packageShopData.productId, type, new IDs() { { packageShopData.productId, GooglePlay.Name } });
                }
            }

            // 패스 상품
            foreach (var passData in Lance.GameData.PassData.Values)
            {
                if (passData.productId.IsValid())
                {
                    builder.AddProduct(passData.productId, ProductType.NonConsumable, new IDs() { { passData.productId, GooglePlay.Name } });
                }
            }

            // 이벤트 패스
            foreach (var eventPassData in Lance.GameData.EventPassData.Values)
            {
                if (eventPassData.productId.IsValid())
                {
                    builder.AddProduct(eventPassData.productId, ProductType.NonConsumable, new IDs() { { eventPassData.productId, GooglePlay.Name } });
                }
            }

            // 코스튬
            foreach(var costumeData in Lance.GameData.BodyCostumeData.Values)
            {
                if (costumeData.productId.IsValid())
                {
                    builder.AddProduct(costumeData.productId, ProductType.NonConsumable, new IDs() { { costumeData.productId, GooglePlay.Name } });
                }
            }

            foreach (var costumeData in Lance.GameData.WeaponCostumeData.Values)
            {
                if (costumeData.productId.IsValid())
                {
                    builder.AddProduct(costumeData.productId, ProductType.NonConsumable, new IDs() { { costumeData.productId, GooglePlay.Name } });
                }
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public bool IsPurchasedRemoveAd()
        {
            // NonConsumable만 유효한 방식
            // Consumable은 계속 구매할 수 있기 때문에
            // 앱을 종료했다가 키면 hasReceipt false 된다.
            if (IsInitialized())
            {
                //string removeAdProductId = DataUtil.GetRemoveAdProductId();

                //Product product = mStoreController.products.WithID(removeAdProductId); //상품 정의

                //return product.hasReceipt;

                return false;
            }
            else
            {
                return false;
            }
        }

        public string GetPrcieString(string id)
        {
            if (IsInitialized() && id.IsValid())
            {
                Product product = mStoreController.products.WithID(id); //상품 정의

                return product?.metadata.localizedPriceString ?? "---";
            }
            else
            {
                return "---";
            }
        }


        /* 구매하는 함수 */
        public void Purchase(string productId, Action onPurchased)
        {
            Product product = mStoreController.products.WithID(productId); //상품 정의

            if (product != null && product.availableToPurchase) //상품이 존재하면서 구매 가능하면
            {
                mOnPurchased = onPurchased;

                mStoreController.InitiatePurchase(product); //구매가 가능하면 진행
            }
            else //상품이 존재하지 않거나 구매 불가능하면
            {
                Debug.Log("상품이 없거나 현재 구매가 불가능합니다");
            }
        }

        bool IsInitialized()
        {
            return mStoreExtensionProvider != null && mStoreController != null;
        }

        #region Interface
        /* 초기화 성공 시 실행되는 함수 */
        public void OnInitialized(IStoreController controller, IExtensionProvider extension)
        {
            Debug.Log("초기화에 성공했습니다");

            mStoreController = controller;
            mStoreExtensionProvider = extension;
        }

        /* 구매를 처리하는 함수 */
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log("구매에 성공했습니다");

            //bool validPurchase = true; // Presume valid for platforms with no R.V.
#if UNITY_EDITOR
            //Lance.TouchBlock.SetActive(true);

            //SendQueue.Enqueue(Backend.Receipt.IsValidateGooglePurchase, args.purchasedProduct.receipt, "receiptDescription", false, (callback) =>
            //{
            //    // 영수증 검증에 성공한 경우
            //    if (callback.IsSuccess())
            //    {
            //        mOnPurchased?.Invoke();
            //    }
            //    else
            //    {
            //        // 영수증 검증에 실패한 경우
            //        UIUtil.ShowSystemErrorMessage("InValidGooglePurchased");
            //    }

            //    Lance.TouchBlock.SetActive(false);
            //});
            //Param param = new Param();
            //param.Add("id", args.purchasedProduct.definition.id);
            //param.Add("storeSpecificId", args.purchasedProduct.definition.storeSpecificId);

            //Lance.BackEnd.InsertLog("PurchasePackageItem", param, 7);

            mOnPurchased?.Invoke();
#elif UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
            ////Unity IAP's validation logic is only included on these platforms.
            ////Prepare the validator with the secrets we prepared in the Editor
            ////obfuscation window.

            //bool validPurchase = true;

            //var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            //    AppleTangle.Data(), Application.identifier);
            //try
            //{
            //    // On Google Play, result has a single product ID.
            //    // On Apple stores, receipts contain multiple products.
            //    IPurchaseReceipt[] result = validator.Validate(args.purchasedProduct.receipt);
            //    // For informational purposes, we list the receipt(s)
            //    Debug.Log("Receipt is valid. Contents:");
            //    foreach (IPurchaseReceipt productReceipt in result)
            //    {
            //        Debug.Log(productReceipt.productID);
            //        Debug.Log(productReceipt.purchaseDate);
            //        Debug.Log(productReceipt.transactionID);
            //    }
            //}
            //catch (IAPSecurityException)
            //{
            //    Debug.Log("Invalid receipt, not unlocking content");
            //    validPurchase = false;
            //}

            //if (validPurchase)
            //{
            //    mOnPurchased?.Invoke();

            //    Param param = new Param();
            //    param.Add("id", args.purchasedProduct.definition.id);
            //    param.Add("storeSpecificId", args.purchasedProduct.definition.storeSpecificId);
            //    param.Add("receipt", args.purchasedProduct.receipt);

            //    Lance.BackEnd.InsertLog("ValidPurchase", param, 7);
            //}

            Lance.MainThreadDispatcher.QueueOnMainThread(() =>
            {
                Lance.TouchBlock.SetActive(true);

                try
                {
                    SendQueue.Enqueue(Backend.Receipt.IsValidateGooglePurchase, args.purchasedProduct.receipt, "receiptDescription", false, (callback) =>
                    {
                        // 영수증 검증에 성공한 경우
                        if (callback.IsSuccess())
                        {
                            mOnPurchased?.Invoke();
                        }
                        else
                        {
                            // 영수증 검증에 실패한 경우
                            UIUtil.ShowSystemErrorMessage("InValidGooglePurchased");
                        }
                    });
                }
                catch 
                {
                    UIUtil.ShowSystemErrorMessage("InValidGooglePurchased");
                }
                finally
                {
                    Lance.TouchBlock.SetActive(false);
                }
            });
#endif
            return PurchaseProcessingResult.Complete;
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            UIUtil.ShowSystemErrorMessage(StringTableUtil.GetSystemMessage("IAPInitializeFailed"), 10f);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            UIUtil.ShowSystemErrorMessage(StringTableUtil.GetSystemMessage("IAPInitializeFailed"), 10f);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {

        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            
        }
#endregion
    }
}