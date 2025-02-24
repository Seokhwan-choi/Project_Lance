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
        IStoreController mStoreController; //���� ������ �����ϴ� �Լ� ������
        IExtensionProvider mStoreExtensionProvider; //���� �÷����� ���� Ȯ�� ó�� ������

        Action mOnPurchased;

        public void Init()
        {
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            /* ���� �÷��� ��ǰ�� �߰� */
            // �� ��ǰ
            foreach (var gemShopData in Lance.GameData.GemShopData.Values)
            {
                if (gemShopData.productId.IsValid())
                {
                    builder.AddProduct(gemShopData.productId, ProductType.Consumable, new IDs() { { gemShopData.productId, GooglePlay.Name } });
                }
            }

            // ��Ű�� ��ǰ
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

            // �н� ��ǰ
            foreach (var passData in Lance.GameData.PassData.Values)
            {
                if (passData.productId.IsValid())
                {
                    builder.AddProduct(passData.productId, ProductType.NonConsumable, new IDs() { { passData.productId, GooglePlay.Name } });
                }
            }

            // �̺�Ʈ �н�
            foreach (var eventPassData in Lance.GameData.EventPassData.Values)
            {
                if (eventPassData.productId.IsValid())
                {
                    builder.AddProduct(eventPassData.productId, ProductType.NonConsumable, new IDs() { { eventPassData.productId, GooglePlay.Name } });
                }
            }

            // �ڽ�Ƭ
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
            // NonConsumable�� ��ȿ�� ���
            // Consumable�� ��� ������ �� �ֱ� ������
            // ���� �����ߴٰ� Ű�� hasReceipt false �ȴ�.
            if (IsInitialized())
            {
                //string removeAdProductId = DataUtil.GetRemoveAdProductId();

                //Product product = mStoreController.products.WithID(removeAdProductId); //��ǰ ����

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
                Product product = mStoreController.products.WithID(id); //��ǰ ����

                return product?.metadata.localizedPriceString ?? "---";
            }
            else
            {
                return "---";
            }
        }


        /* �����ϴ� �Լ� */
        public void Purchase(string productId, Action onPurchased)
        {
            Product product = mStoreController.products.WithID(productId); //��ǰ ����

            if (product != null && product.availableToPurchase) //��ǰ�� �����ϸ鼭 ���� �����ϸ�
            {
                mOnPurchased = onPurchased;

                mStoreController.InitiatePurchase(product); //���Ű� �����ϸ� ����
            }
            else //��ǰ�� �������� �ʰų� ���� �Ұ����ϸ�
            {
                Debug.Log("��ǰ�� ���ų� ���� ���Ű� �Ұ����մϴ�");
            }
        }

        bool IsInitialized()
        {
            return mStoreExtensionProvider != null && mStoreController != null;
        }

        #region Interface
        /* �ʱ�ȭ ���� �� ����Ǵ� �Լ� */
        public void OnInitialized(IStoreController controller, IExtensionProvider extension)
        {
            Debug.Log("�ʱ�ȭ�� �����߽��ϴ�");

            mStoreController = controller;
            mStoreExtensionProvider = extension;
        }

        /* ���Ÿ� ó���ϴ� �Լ� */
        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            Debug.Log("���ſ� �����߽��ϴ�");

            //bool validPurchase = true; // Presume valid for platforms with no R.V.
#if UNITY_EDITOR
            //Lance.TouchBlock.SetActive(true);

            //SendQueue.Enqueue(Backend.Receipt.IsValidateGooglePurchase, args.purchasedProduct.receipt, "receiptDescription", false, (callback) =>
            //{
            //    // ������ ������ ������ ���
            //    if (callback.IsSuccess())
            //    {
            //        mOnPurchased?.Invoke();
            //    }
            //    else
            //    {
            //        // ������ ������ ������ ���
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
                        // ������ ������ ������ ���
                        if (callback.IsSuccess())
                        {
                            mOnPurchased?.Invoke();
                        }
                        else
                        {
                            // ������ ������ ������ ���
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