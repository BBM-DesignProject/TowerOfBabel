// TestLevelUpUI.cs
using UnityEngine;
using System.Collections.Generic; // List<T> için

public class TestLevelUpUI : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) // L tuþuna basýnca paneli göster
        {
            if (UIManager.Instance != null)
            {
                List<UIManager.SpellUpgradeChoice> testChoices = new List<UIManager.SpellUpgradeChoice>();

                testChoices.Add(new UIManager.SpellUpgradeChoice
                {
                    spellName = "Ateþ Topu Güçlendirmesi",
                    description = "Ateþ topu hasarýný %25 artýrýr.",
                    choiceIndex = 0 // Bu, sizin sisteminizde bir Spell ID veya Upgrade ID olabilir
                });

                testChoices.Add(new UIManager.SpellUpgradeChoice
                {
                    spellName = "Hýzlý Koþu",
                    description = "Hareket hýzýný 5 saniyeliðine artýrýr.",
                    choiceIndex = 1
                });

                testChoices.Add(new UIManager.SpellUpgradeChoice
                {
                    spellName = "Can Yenileme",
                    description = "Anýnda 20 can yeniler.",
                    choiceIndex = 2
                });
                // Ýsterseniz daha az seçenek de gönderebilirsiniz, UIManager kullanýlmayan slotlarý gizleyecektir.

                UIManager.Instance.ShowLevelUpPanel(testChoices);
            }
            else
            {
                Debug.LogError("UIManager.Instance is null!");
            }
        }
    }
}
