using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditButton : SettingUI {
    [SerializeField]
    private GameObject creditObject;
    private bool isOpen = false;

    public override void Execute() {
        isOpen = !isOpen;
        creditObject.gameObject.SetActive(isOpen);

        UpdateCoroutine().Start(this);
    }

    private IEnumerator UpdateCoroutine() {
        while (true) {
            creditObject.gameObject.SetActive(isOpen);
            yield return YieldInstructionCache.WaitFrame;
        }
    }
}
