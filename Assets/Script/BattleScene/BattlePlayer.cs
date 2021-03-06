﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class BattlePlayer : CommonBattleChara
{    
    new void Start()
    {
        HP = 30;
        attack = 2;
        defaultOffset = new Vector2(0, 1);
        hpberOffset = new Vector2(0, -1.32f);

        attackText = new string[3] {"通常攻撃", "バーチカルアタック", "スラッシュスラント"};
        idleText = new string[1] { "昼寝をする" };
        base.Start();
    }
    
    void Update()
    {
        
    }

    public void SetOnClickAttack()
    {
        UnityAction[] method = new UnityAction[] { OnNormalAttack, OnAttackMoveVertical, OnAttackMoveSlash};
        SetMethodAttack(method);
    }

    public void SetOnClickIdle()
    {
        UnityAction[] method = new UnityAction[] { OnIdle };
        SetMethodIdle(method);
    }

    /*以下ボタン関数*/

    public void OnNormalAttack()
    {
        if (isCommandPushed)
            return;

        Vector2 movedPos = ConvertObjectToVector(gameObject);

        //攻撃し、その場に留まるので
        movedPos.y = 1;

        if (ConvertVectorToObject(movedPos) == null)
        {
            BattleManager.instance.AddMessage(messageList.nonTarget);
            soundBox.PlayOneShot(audioClass.notExecute, 1f);
            return;
        }

        isCommandPushed = true;
        BattleManager.instance.stackCommandPlayer = new BattleManager.StackCommandPlayer(NormalAttack);
        BattleManager.instance.soundBox.PlayOneShot(audioClass.decide, 1f);
        BattleManager.instance.ChangeTurnNext();
    }

    private void NormalAttack()
    {
        Vector2 movedPos = ConvertObjectToVector(gameObject);
        movedPos.y = 1;

        var moveHash = new Hashtable();
        //gridPOsiiotnsだとnullのときエラーが出るので
        moveHash.Add("x", BattleManager.instance.basePositions[(int)movedPos.x, (int)movedPos.y].transform.position.x);
        moveHash.Add("time", charMoveTime);
        iTween.MoveFrom(gameObject, moveHash);

        AnimStart(null, audioClass.normalAttack);
        BattleManager.instance.AddMessage(objectName + "の通常攻撃!");
        //Invoke("DelayChange", BattleManager.instance.changeTurnWaitTime);
        if (ConvertVectorToObject(movedPos) != null)
        {
            ConvertVectorToObject(movedPos).GetComponent<CommonBattleChara>().DamagedAnim(attack);
        }
        
    }

    public void OnAttackMoveVertical()
    {
        if (isCommandPushed)
            return;

        isCommandPushed = true;
        BattleManager.instance.stackCommandPlayer = new BattleManager.StackCommandPlayer(AttackMoveVertical);
        BattleManager.instance.soundBox.PlayOneShot(audioClass.decide, 1f);
        BattleManager.instance.ChangeTurnNext();
    }

    private void AttackMoveVertical()
    {
        Vector2 movedPos = ConvertObjectToVector(gameObject);

        //向かい側に移動するのでyだけ動かす
        if (movedPos.y == 0)
            movedPos.y = 2;
        else if (movedPos.y == 2)
            movedPos.y = 0;

        if (CanChangeGrid(movedPos))
        {
            Vector2 target = movedPos;
            target.y = 1;

            if (ConvertVectorToObject(target) != null)
            {
                GameObject effect = Instantiate(Resources.Load("Effecter")) as GameObject;
                if(transform.localScale.x > 0)
                    effect.transform.localScale = new Vector3(5f, 5f, 1f);
                else
                    effect.transform.localScale = new Vector3(-5f, 5f, 1f);

                effect.transform.position = ConvertVectorToObject(target).transform.position;
                OnlyAnim(effect, controller[1], audioClass.slash, objectName + "の" + attackText[1] + "!");
                //effect.GetComponent<Animator>().runtimeAnimatorController = controller[1];
                //effect.GetComponent<Animator>().SetTrigger("Start");
                //soundBox.PlayOneShot(null, 1f);
                Destroy(effect, 2f);
            }

            if (ConvertVectorToObject(target) != null)
            {
                if (ConvertVectorToObject(movedPos) != null && ConvertVectorToObject(movedPos).tag == "Player")
                    ConvertVectorToObject(target).GetComponent<CommonBattleChara>().DamagedAnim(attack * 4);
                else
                    ConvertVectorToObject(target).GetComponent<CommonBattleChara>().DamagedAnim(attack);
            }
            ChangeGrid(gameObject, movedPos);
            BattleManager.instance.AddMessage(objectName + "の" + attackText[1] + "!");
        }
        else
        {
            print(messageList.nonMove);
        }
    }

    public void OnAttackMoveSlash()
    {
        if (isCommandPushed)
            return;

        isCommandPushed = true;
        BattleManager.instance.stackCommandPlayer = new BattleManager.StackCommandPlayer(AttackMoveSlash);
        BattleManager.instance.soundBox.PlayOneShot(audioClass.decide, 1f);
        BattleManager.instance.ChangeTurnNext();
    }

    protected void AttackMoveSlash()
    {
        Vector2 movedPos = ConvertObjectToVector(gameObject);
        
        //対角に移動するので0と2を反転
        if (movedPos.x == 0)
            movedPos.x = 2;
        else if (movedPos.x == 2)
            movedPos.x = 0;

        if (movedPos.y == 0)
            movedPos.y = 2;
        else if (movedPos.y == 2)
            movedPos.y = 0;

        if (CanChangeGrid(movedPos))
        {
            Vector2 target = new Vector2(1, 1);
            if (ConvertVectorToObject(target) != null)
            {
                GameObject effect = Instantiate(Resources.Load("Effecter")) as GameObject;
                if (ConvertObjectToVector(gameObject) == new Vector2(0, 0))
                {
                    effect.transform.localScale = new Vector3(-5f, 5f, 1f);
                    effect.transform.Rotate(new Vector3(0f, 0f, -40f));
                }
                else if (ConvertObjectToVector(gameObject) == new Vector2(2, 0))
                {
                    effect.transform.localScale = new Vector3(-5f, 5f, 1f);
                    effect.transform.Rotate(new Vector3(0f, 0f, 25f));
                }
                else if (ConvertObjectToVector(gameObject) == new Vector2(0, 2))
                {
                    effect.transform.localScale = new Vector3(5f, 5f, 1f);
                    effect.transform.Rotate(new Vector3(0f, 0f, 25f));
                }
                else if (ConvertObjectToVector(gameObject) == new Vector2(2, 2))
                {
                    effect.transform.localScale = new Vector3(5f, 5f, 1f);
                    effect.transform.Rotate(new Vector3(0f, 0f, -40f));
                }

                effect.transform.position = ConvertVectorToObject(target).transform.position;
                OnlyAnim(effect, controller[1], audioClass.slash, objectName + "の" + attackText[2] + "!");
                //effect.GetComponent<Animator>().runtimeAnimatorController = controller[1];
                //effect.GetComponent<Animator>().SetTrigger("Start");
                //soundBox.PlayOneShot(null, 1f);
                Destroy(effect, 2f);
            }

            if (ConvertVectorToObject(target) != null)
            {
                if (ConvertVectorToObject(movedPos) != null && ConvertVectorToObject(movedPos).tag == "Player")
                    ConvertVectorToObject(target).GetComponent<CommonBattleChara>().DamagedAnim(attack * 5);
                else
                    ConvertVectorToObject(target).GetComponent<CommonBattleChara>().DamagedAnim(attack);
            }
            ChangeGrid(gameObject, movedPos);
            BattleManager.instance.AddMessage(objectName + "の" + attackText[2] + "!");
        }
        else
        {
            print(messageList.nonMove);
            soundBox.PlayOneShot(audioClass.notExecute, 1f);
        }
    }

    public void OnIdle()
    {
        if (isCommandPushed)
            return;

        isCommandPushed = true;
        BattleManager.instance.stackCommandPlayer = new BattleManager.StackCommandPlayer(Idle);
        BattleManager.instance.soundBox.PlayOneShot(audioClass.decide, 1f);
        BattleManager.instance.ChangeTurnNext();
    }

    private void Idle()
    {
        BattleManager.instance.AddMessage(objectName + "は昼寝をした");
    }    
}
