﻿using RealtimeArena.Battle;
using UnityEngine;
using UnityEngine.UI;

namespace RealtimeArena.UI
{
    [RequireComponent(typeof(UICharacterStatsGeneric))]
    public class UIPlayerDecisionWaiting : MonoBehaviour
    {
        public GameObject rootContainer;
        public Text textCountDown;
        public Image imageCountDownGage;
        private UICharacterStatsGeneric uiCharacterStats;
        private RealtimeArenaGameplayManager manager;
        private bool dirtyIsActiveCharacter;
        private float countDown;

        private void Start()
        {
            uiCharacterStats = GetComponent<UICharacterStatsGeneric>();
            manager = FindObjectOfType<RealtimeArenaGameplayManager>();
            StartCountDown();
        }

        private void Update()
        {
            if (!uiCharacterStats.character)
            {
                if (rootContainer)
                    rootContainer.SetActive(false);
                return;
            }

            if (dirtyIsActiveCharacter != (uiCharacterStats.character.IsActiveCharacter && manager.WaitingForAction))
            {
                dirtyIsActiveCharacter = (uiCharacterStats.character.IsActiveCharacter && manager.WaitingForAction);
                StartCountDown();
            }

            countDown -= Time.deltaTime;
            if (countDown <= 0f)
                countDown = 0f;

            var rate = countDown / manager.decisionWaitingDuration;

            if (textCountDown != null)
                textCountDown.text = countDown <= 0 ? "" : countDown.ToString("N0");

            if (imageCountDownGage != null)
                imageCountDownGage.fillAmount = rate;

            if (rootContainer)
                rootContainer.SetActive(uiCharacterStats.character.IsActiveCharacter && countDown > 0f && manager.WaitingForAction);
        }

        public void StartCountDown()
        {
            countDown = manager.decisionWaitingDuration;
        }
    }
}
