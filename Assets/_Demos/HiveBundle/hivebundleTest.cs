using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UFrame.HiveBundle;

public class hivebundleTest : MonoBehaviour
{
    public string bundleName;
    private AssetBundleCtrl bundleCtrl;
    // Start is called before the first frame update
    void Start()
    {
        bundleCtrl = new AssetBundleCtrl(true);
        bundleCtrl.SetLocalFiles(new Dictionary<string, string>() { { bundleName,""} });

        bundleCtrl.PreloadAssetBundle(bundleName);
        bundleCtrl.InstantiateAll(bundleName);
    }
}
