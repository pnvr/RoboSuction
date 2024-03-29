﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct LevelData {
    public int numberOfEnemies;
    public float spawnInterval;
    public GameObject[] enemies;
    public GameObject[] spawnpoints;

    public LevelData(int nmbrfnms, float spwnntrvl, GameObject[] nms, GameObject[] spwnpts) {
        numberOfEnemies = nmbrfnms;
        spawnInterval = spwnntrvl;
        enemies = nms;
        spawnpoints = spwnpts;
    }
}

public class GameManager : MonoBehaviour {

    public TextMeshProUGUI uiText;
    public TextMeshProUGUI wave1;
    public TextMeshProUGUI wave2;
    public TextMeshProUGUI kills1;
    public TextMeshProUGUI kills2;

    public LevelData[] leveldata;
    public GameObject space;
    public GameObject[] particles;
    public GameObject[] pilars;
    public List<string> infoTexts;

    public AudioSource gmAudio;
    public AudioClip[] narration;

    public Animator anim;

    public int enemiesSpawned;
    public int enemiesKilled = 0;
    public int waveNumber = 0;

    public float resetTimer;
    public float resetTime = 5;
    float textTimer;
    float textTime = 5f;
    float spawnTimer = 5f;

    public GameObject player;
    public Slider healthBar;

    public int playerHealth = 100;
    public bool resetComing;

    public bool hasGameStarted;
    public bool hasGameEnded;
    public int sticksInHand;

    void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            anim.Play("End_Animation");
        }
        if(!hasGameEnded && hasGameStarted) {
            spawnTimer -= Time.deltaTime;
            textTimer -= Time.deltaTime;

            if(textTimer < 0) {
                uiText.text = "";
            }

            if(EnemiesToSpawn() && spawnTimer < 0) {
                EnemySpawner();
            }

            if(!EnemiesToSpawn() && !MoreEnemiesSpawnedThanKilled()) {
                if (!resetComing) {
                    PlayLevelAudio(4);
                    resetComing = true;
                }

                resetTimer -= Time.deltaTime;
                SetUIText(infoTexts[infoTexts.Count - 3]);
                textTimer = textTime;
                if(resetTimer < 0) {
                    ResetWave();
                    resetTimer = resetTime;
                }
            }
        }
    }

    public void ResetWave() {
        resetComing = false;
        if(IsLastRound() && !MoreEnemiesSpawnedThanKilled()) {
            hasGameEnded = true;
        }

        if(!hasGameEnded) {
            if(!IsLastRound())
                waveNumber++;
            enemiesSpawned = 0;
            enemiesKilled = 0;
            SetUIText(infoTexts[waveNumber]);
            PlayLevelAudio(waveNumber);    
            SetKillText();
            SetWaveText();
            textTimer = textTime;
            spawnTimer = textTime;
            particles[waveNumber].SetActive(true);

            if(waveNumber == leveldata.Length-1) {
                var pilarTruth = new bool[] { false, true, true, true, true, true, true, false, true, false, false, false, false, true, false, true };
                for (int i = 0; i < pilars.Length; i++){
                    pilars[i].SetActive(pilarTruth[i]);
                }

                //pilars[0].SetActive(false);
                //pilars[1].SetActive(true);
                //pilars[2].SetActive(true);
                //pilars[3].SetActive(true);
                //pilars[4].SetActive(true);
                //pilars[5].SetActive(true);
                //pilars[6].SetActive(true);
                //pilars[7].SetActive(false);
                //pilars[8].SetActive(true);
                //pilars[9].SetActive(false);
                //pilars[10].SetActive(false);
                //pilars[11].SetActive(false);
                //pilars[12].SetActive(false);
                //pilars[13].SetActive(true);
                //pilars[14].SetActive(false);
                //pilars[15].SetActive(true);

            }
        } else {
            foreach (var go in particles){
                go.SetActive(false);
            }
            SetUIText(infoTexts[infoTexts.Count - 2]);
            PlayLevelAudio(6);
            anim.Play("End_Animation");
        }
    }

    public bool EnemiesToSpawn() {
        return leveldata[waveNumber].numberOfEnemies > enemiesSpawned && !hasGameEnded;
    }

    public bool IsLastRound() {
        return waveNumber >= leveldata.Length - 1;
    }

    public bool MoreEnemiesSpawnedThanKilled() {
        return enemiesKilled < enemiesSpawned;
    }

    public void EnemySpawner() {
        var spawnpointIndex = Random.Range(0, leveldata[waveNumber].spawnpoints.Length);
        var spawnPoint = leveldata[waveNumber].spawnpoints[spawnpointIndex].transform.position;
        var enemyIndex = Random.Range(0, leveldata[waveNumber].enemies.Length);
        var enemy = leveldata[waveNumber].enemies[enemyIndex];
        var f = Vector3.ProjectOnPlane(player.transform.position - spawnPoint, Vector3.up);
        Instantiate(enemy, spawnPoint, Quaternion.LookRotation(f));

        spawnTimer = leveldata[waveNumber].spawnInterval;
        enemiesSpawned++;
    }

    public void SetHealth(int amount) {

        playerHealth += amount;
        if(playerHealth > 100)
            playerHealth = 100;
        var dh = Mathf.Ceil(playerHealth / 20f);

        healthBar.value = dh / 5;
        if(playerHealth < 1) {
            PlayLevelAudio(5);
            uiText.text = infoTexts[infoTexts.Count - 1];
            textTimer = textTime;
            hasGameEnded = true;
            Time.timeScale = 0;
        }
    }

    public void CountSticksInHand(int i) {
        sticksInHand += i;
        if(sticksInHand > 1 && !hasGameStarted) {
            hasGameStarted = true;
            StartGame();
        }
    }

    void StartGame() {
        uiText.text = infoTexts[waveNumber];
        SetWaveText();
        SetKillText();
        textTimer = textTime;
        PlayLevelAudio(waveNumber);
        gmAudio.Play();
        space.SetActive(true);
    }

    void PlayLevelAudio(int i) {
        gmAudio.PlayOneShot(narration[i]);
    }

    public void SetUIText(string text) {
        uiText.text = text;
    }

    public void SetKillText() {
        kills1.text = "Kills:" + enemiesKilled;
        kills2.text = "Kills:" + enemiesKilled;
    }

    public void SetWaveText() {
        wave1.text = "Wave:" + waveNumber;
        wave2.text = "Wave:" + waveNumber;
    }
}
