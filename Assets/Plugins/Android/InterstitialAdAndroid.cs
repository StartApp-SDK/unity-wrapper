﻿/*
Copyright 2019 StartApp Inc

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

#if UNITY_ANDROID

using UnityEngine;

namespace StartApp
{
    public class InterstitialAdAndroid : InterstitialAd
    {
        #if !UNITY_EDITOR
            readonly AndroidJavaObject mjStartAppAd;
        #endif

        readonly string mAdTag;

        static InterstitialAdAndroid()
        {
            #if !UNITY_EDITOR
                AdSdkAndroid.ImplInstance.Setup();
            #endif
        }

        public InterstitialAdAndroid(string tag = null)
        {
            #if !UNITY_EDITOR
                mjStartAppAd = new AndroidJavaObject("com.startapp.sdk.adsbase.StartAppAd", AdSdkAndroid.ImplInstance.Activity);
                mjStartAppAd.Call("setVideoListener", new ImplementationVideoListener(this));
                mAdTag = tag;
            #endif
        }

        public override void LoadAd(AdType mode)
        {
            #if !UNITY_EDITOR
                var adprefs = new AndroidJavaObject("com.startapp.sdk.adsbase.model.AdPreferences");
                if (mAdTag != null)
                {
                    adprefs.Call<AndroidJavaObject>("setAdTag", mAdTag);
                }

                mjStartAppAd.Call("loadAd", GetJAdType(mode), adprefs, new ImplementationAdEventListener(this));
            #endif
        }

        public override bool ShowAd()
        {
            #if !UNITY_EDITOR
                return mjStartAppAd.Call<bool>("showAd", new ImplementationAdDisplayListener(this));
            #else
                return false;
            #endif
        }

        public override bool IsReady()
        {
            #if !UNITY_EDITOR
                return mjStartAppAd.Call<bool>("isReady");
            #else
                return false;
            #endif
        }

    #if !UNITY_EDITOR
        class ImplementationAdEventListener : AndroidJavaProxy
        {
            readonly InterstitialAdAndroid mParent;

            public ImplementationAdEventListener(InterstitialAdAndroid parent) : base("com.startapp.sdk.adsbase.adlisteners.AdEventListener")
            {
                mParent = parent;
            }

            void onReceiveAd(AndroidJavaObject ad)
            {
                mParent.OnRaiseAdLoaded();
            }

            void onFailedToReceiveAd(AndroidJavaObject ad)
            {
                var errorMessage = ad.Call<AndroidJavaObject>("getErrorMessage");
                mParent.OnRaiseAdLoadingFailed(errorMessage.Call<string>("toString"));
            }
        }

        class ImplementationAdDisplayListener : AndroidJavaProxy
        {
            readonly InterstitialAdAndroid mParent;

            public ImplementationAdDisplayListener(InterstitialAdAndroid parent) : base("com.startapp.sdk.adsbase.adlisteners.AdDisplayListener")
            {
                mParent = parent;
            }

            void adHidden(AndroidJavaObject ad)
            {
                mParent.OnRaiseAdClosed();
            }

            void adDisplayed(AndroidJavaObject ad)
            {
                mParent.OnRaiseAdShown();
            }

            void adClicked(AndroidJavaObject ad)
            {
                mParent.OnRaiseAdClicked();
            }
        }

        class ImplementationVideoListener : AndroidJavaProxy
        {
            readonly InterstitialAdAndroid mParent;

            public ImplementationVideoListener(InterstitialAdAndroid parent) : base("com.startapp.sdk.adsbase.VideoListener")
            {
                mParent = parent;
            }

            void onVideoCompleted()
            {
                mParent.OnRaiseAdVideoCompleted();
            }
        }

        static AndroidJavaObject GetJAdType(AdType adMode)
        {
            var jModeClass = new AndroidJavaClass("com.startapp.sdk.adsbase.StartAppAd$AdMode");
            switch (adMode)
            {
                case AdType.Automatic: return jModeClass.GetStatic<AndroidJavaObject>("AUTOMATIC");
                case AdType.FullScreen: return jModeClass.GetStatic<AndroidJavaObject>("FULLPAGE");
                case AdType.OfferWall: return jModeClass.GetStatic<AndroidJavaObject>("OFFERWALL");
                case AdType.Rewarded: return jModeClass.GetStatic<AndroidJavaObject>("REWARDED_VIDEO");
                case AdType.Video: return jModeClass.GetStatic<AndroidJavaObject>("VIDEO");
            }

            return jModeClass.GetStatic<AndroidJavaObject>("AUTOMATIC");
        }
    #endif
    }
}

#endif