using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AnimationManager : Singleton<AnimationManager>
{

    public bool bgAnimation;
    public bool STOP_ALPHA = false;



    public IEnumerator ShinePU(bool repeat, float delayBefore, int delayForNextShine, Material material, Action OnFinnish)
    {
        float shineDuration = Values.Instance.shineDuration;
        float shineLocation = 1f;
        yield return new WaitForSeconds(delayBefore);

        material.SetFloat("_ShineGlow", 1.5f);

        while (shineLocation > 0)
        {
            shineLocation -= Time.deltaTime / shineDuration;
            material.SetFloat("_ShineLocation", shineLocation);
            yield return new WaitForFixedUpdate();
            if (shineLocation <= 0)
            {
                material.SetFloat("_ShineGlow", 0f);
                if (repeat)
                {
                    yield return new WaitForSeconds(delayForNextShine);
                }
                OnFinnish?.Invoke();
                break;
            }
        }
    }

    public IEnumerator UpdateValue(bool increase, string valueName, float duration, Material material, float target, Action OnFinnish)
    {
        float valueToUpdate = material.GetFloat(valueName);
        if (valueToUpdate != target)
        {
            if (!increase)
            {
                while (valueToUpdate < target)
                {
                    valueToUpdate += Time.deltaTime / duration;
                    material.SetFloat(valueName, valueToUpdate);
                    yield return new WaitForFixedUpdate();
                    /*  if (valueToUpdate >= target)
                      {
                          material.SetFloat(valueName, target);
                          OnFinnish?.Invoke();
                          Debug.LogWarning("CHECKME");
                          break;
                      }*/
                }
            }
            else
            {
                while (valueToUpdate > target)
                {
                    valueToUpdate -= Time.deltaTime / duration;
                    material.SetFloat(valueName, valueToUpdate);
                    yield return new WaitForFixedUpdate();
                    /* if (valueToUpdate <= target)
                     {
                         material.SetFloat(valueName, target);
                         OnFinnish?.Invoke();
                         break;
                     }*/
                }
            }

            material.SetFloat(valueName, target);
        }
        OnFinnish?.Invoke();
    }


    public void FadeBurnDarkScreen(Material darkScreenMaterial, bool enable, Action ResetSortingOrder)
    {
        StartCoroutine(FadeDarkScreen(darkScreenMaterial, enable, Values.Instance.darkScreenAlphaDuration, ResetSortingOrder));
    }



    public IEnumerator Shake(Material material)
    {

        if (material.GetFloat("_ShakeUvSpeed") == 0)
        {

            material.SetFloat("_ShakeUvSpeed", Values.Instance.disableClickShakeSpeed);
            material.SetFloat("_ShakeUvX", Values.Instance.disableClickShakeX);
            material.SetFloat("_ShakeUvY", Values.Instance.disableClickShakeY);
            yield return new WaitForSeconds(Values.Instance.disableClickShakeDuration);
            material.SetFloat("_ShakeUvSpeed", 0f);
        }

    }


    public IEnumerator PulseSize(bool enable, Transform selector, float scaleTarget, float flipDuration, bool isAfter, Action nextRoutine)
    {

        float startTime = Time.time;
        float t;
        bool finishPulse = false;
        bool shrinking = false;
        Vector3 vectorTargetFinal = new Vector3(selector.localScale.x, selector.localScale.y, selector.localScale.z);
        Vector3 vectorTargetInLarge = new Vector3(selector.localScale.x * scaleTarget, selector.localScale.y * scaleTarget, selector.localScale.z);
        Vector3 target = vectorTargetInLarge;
        if (!isAfter)
        {
            nextRoutine?.Invoke();
        }
        if (enable)
        {
            while (!finishPulse)
            {
                t = (Time.time - startTime) / flipDuration;
                if (shrinking)
                {
                    target = vectorTargetFinal;
                    if (selector.localScale.x == vectorTargetFinal.x)
                    {
                        finishPulse = true;
                    }
                }
                selector.localScale = new Vector3(Mathf.SmoothStep(selector.localScale.x, target.x, t), Mathf.SmoothStep(selector.localScale.y, target.y, t), target.z);
                if (selector.localScale.x == vectorTargetInLarge.x)
                {
                    shrinking = true;
                    startTime = Time.time;
                }
                yield return new WaitForFixedUpdate();

            }
        }
        if (isAfter)
        {
            nextRoutine?.Invoke();
        }
        yield break;
    }


    public IEnumerator AlphaAnimation(SpriteRenderer spriteRenderer, bool fadeIn, float duration, Action OnFinish)
    {
            Debug.LogError("sp " + spriteRenderer.gameObject.name);
        float r = spriteRenderer.color.r;
        float g = spriteRenderer.color.g;
        float b = spriteRenderer.color.b;
        float dissolveAmount = 1;
        float alphaTarget = 0;
        if (fadeIn)
        {
            dissolveAmount = 0;
            alphaTarget = 1;
        }
        spriteRenderer.color = new Color(r, g, b, dissolveAmount);
        //FIXIT
        while (dissolveAmount != alphaTarget)
        {
            yield return new WaitForFixedUpdate();
            if (fadeIn)
            {
                dissolveAmount += Time.deltaTime / duration;
            }
            else
            {
                dissolveAmount -= Time.deltaTime / duration;
            }

            spriteRenderer.color = new Color(r, g, b, Mathf.Lerp(0f, 1f, dissolveAmount));
            if (dissolveAmount >= 1 || dissolveAmount <=0)
            {
               // Debug.LogError("NEEDTOFIX? " + dissolveAmount);
               // Debug.LogError("NEEDTOFIX " + spriteRenderer.gameObject.name);

                spriteRenderer.color = new Color(r, g, b, Mathf.Lerp(0f, 1f, alphaTarget));
                OnFinish?.Invoke();
                break;
            }
        }
    }
    public IEnumerator AlphaFadeIn(SpriteRenderer spriteRenderer, float duration, Action OnFinish)
    {
        float r = spriteRenderer.color.r;
        float g = spriteRenderer.color.g;
        float b = spriteRenderer.color.b;
        float dissolveAmount = 0;
        float alphaTarget = 1;
       
        spriteRenderer.color = new Color(r, g, b, 0f);
        while (dissolveAmount < alphaTarget)
        {
            yield return new WaitForFixedUpdate();
            
                dissolveAmount += Time.deltaTime / duration;
            

            spriteRenderer.color = new Color(r, g, b, Mathf.Lerp(0f, 1f, dissolveAmount));
            if (dissolveAmount >= alphaTarget)
            {
                spriteRenderer.color = new Color(r, g, b, 1f);
                OnFinish?.Invoke();
                break;
            }
        }
     
    }

    public IEnumerator AlphaFadeOut(SpriteRenderer spriteRenderer, float duration, Action OnFinish)
    {
        float r = spriteRenderer.color.r;
        float g = spriteRenderer.color.g;
        float b = spriteRenderer.color.b;
        float dissolveAmount = 1;
        float alphaTarget = 0;

        spriteRenderer.color = new Color(r, g, b, 1f);
        while (dissolveAmount > alphaTarget)
        {
            yield return new WaitForFixedUpdate();
           
                dissolveAmount -= Time.deltaTime / duration;
            

            spriteRenderer.color = new Color(r, g, b, Mathf.Lerp(0f, 1f, dissolveAmount));
            if (dissolveAmount <= alphaTarget)
            {
                spriteRenderer.color = new Color(r, g, b, 0f);
                OnFinish?.Invoke();
                break;
            }
        }

    }
    public IEnumerator AlphaFontAnimation(TMPro.TextMeshProUGUI txtMesh, bool fadeIn, float duration, Action OnFinish)
    {
        float r = txtMesh.color.r;
        float g = txtMesh.color.g;
        float b = txtMesh.color.b;
        float dissolveAmount = 1;
        float alphaTarget = 0;
        if (fadeIn)
        {
            dissolveAmount = 0;
            alphaTarget = 1;
        }


        while (txtMesh.color.a != alphaTarget)
        {

            yield return new WaitForFixedUpdate();
            if (fadeIn)
            {
                dissolveAmount += Time.deltaTime / duration;
            }
            else
            {
                dissolveAmount -= Time.deltaTime / duration;
            }

            txtMesh.color = new Color(r, g, b, Mathf.Lerp(0f, 1f, dissolveAmount));
            if (txtMesh.color.a == alphaTarget)
            {
                OnFinish?.Invoke();
                break;
            }
        }

        if (txtMesh.color.a == alphaTarget)
        {
            OnFinish?.Invoke();
        }
    }

    public IEnumerator FreezeEffect(bool freeze, bool isFaceDown, SpriteRenderer targetObj, Material targetMaterial, Action onFinishDissolve)
    {
        float freezeDuration = Values.Instance.FreezeDuration;
        targetObj.material = targetMaterial;
        // targetObj.material.SetColor("_FadeBurnColor", Color.blue);
        // float tiling = UnityEngine.Random.Range(0.2f, 0.4f);
        // targetObj.material.SetTextureScale("_FadeTex", new Vector2(tiling, tiling));
        float fullFreezeAmount = 0.5f;
        float dissolveAmount = -0.1f;
        if (isFaceDown)
        {
            fullFreezeAmount = 0.6f;
            targetObj.material.SetFloat("_FadeBurnGlow", 2);
        }
        else
        {
            targetObj.material.SetFloat("_FadeBurnGlow", 1);
        }
        if (!freeze)
        {
            dissolveAmount = fullFreezeAmount;
        }
        while (dissolveAmount <= fullFreezeAmount && dissolveAmount >= -0.1f)
        {
            if (freeze)
            {
                dissolveAmount += Time.deltaTime / freezeDuration;
            }
            else
            {
                dissolveAmount -= Time.deltaTime / freezeDuration;
            }
            targetObj.material.SetFloat("_FadeAmount", dissolveAmount);
            if (dissolveAmount >= fullFreezeAmount || dissolveAmount <= -0.1f)
            {
                onFinishDissolve?.Invoke();
                if (!freeze)
                {
                    targetObj.material = targetMaterial;
                    targetObj.material.SetFloat("_FadeAmount", -0.1f);
                }
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        yield return new WaitForFixedUpdate();
    }

    internal IEnumerator LoseLifeAnimation(SpriteRenderer spriteRenderer, Action Fadeout)
    {
        float duration = Values.Instance.LoseLifeDuration;

        float brightnesstAmount = 0f;
        bool increas = true;
        bool fadeout = false;

        while (brightnesstAmount > -1f && brightnesstAmount < 1f)
        {
            if (increas)
            {
                brightnesstAmount += Time.deltaTime / duration;
            }
            else
            {
                brightnesstAmount -= Time.deltaTime / duration;
            }
            spriteRenderer.material.SetFloat("_Brightness", brightnesstAmount);
            yield return new WaitForFixedUpdate();
            if (increas && brightnesstAmount >= 0.9f)
            {
                increas = false;
                yield return new WaitForSeconds(0.05f);
                //  Fadeout?.Invoke();

            }
            if (!increas && !fadeout && brightnesstAmount < 0)
            {
                fadeout = true;
                Fadeout?.Invoke();
            }
            if (brightnesstAmount <= -1f)
            {
                break;
            }
        }
    }



    public IEnumerator SmoothMove(Transform selector, Vector3 targetPosition, Vector2 targetScale, float movementDuration, Action beginAction, Action endAction, Action Reset, Action CloseDrawer)
    {
        beginAction?.Invoke();
        float startTime = Time.time;
        float t;
        bool endLoop = false;
        float speed = 1.5f;
        while (selector.position != targetPosition || (Vector2)selector.localScale != targetScale)
        {
            t = (Time.time - startTime) / movementDuration;
            selector.position = new Vector3(Mathf.SmoothStep(selector.position.x, targetPosition.x, t * speed), Mathf.SmoothStep(selector.position.y, targetPosition.y, t * speed), targetPosition.z);
            if (selector.localScale.x != targetScale.x)
            {
                selector.localScale = new Vector3(Mathf.SmoothStep(selector.localScale.x, targetScale.x, t * speed), Mathf.SmoothStep(selector.localScale.y, targetScale.y, t * speed), 1f);
            }
            yield return new WaitForFixedUpdate();
            if (!endLoop && selector.position == targetPosition && (Vector2)selector.localScale == targetScale)
            {
                endLoop = true;
                endAction?.Invoke();
                Reset?.Invoke();
                CloseDrawer?.Invoke();
                break;
            }
        }
        yield break;
    }



    public IEnumerator SmoothMoveCardProjectile(bool isPlayerWin, bool isRight, Transform selector, Vector2 targetScale, float movementDuration, Action LoseLife, Action Unactive)
    {
        Vector3 targetPosition;
        float startTime = Time.time;
        float t = 0;
        bool endLoop = false;
        if (isPlayerWin)
        {
            targetPosition = Values.Instance.winCardRightEnd.position;
            if (!isRight)
            {
                targetPosition = Values.Instance.winCardLeftEnd.position;
            }
        }
        else
        {
            targetPosition = Values.Instance.winCardRightStart.position;
            if (!isRight)
            {
                targetPosition = Values.Instance.winCardLeftStart.position;
            }

        }
        // selector.position = startingPosition;
        while (selector.position != targetPosition || (Vector2)selector.localScale != targetScale)
        {
            // t = (Time.time - startTime) / movementDuration;
            t = (Time.time - startTime) / movementDuration;
            selector.position = new Vector3(Mathf.SmoothStep(selector.position.x, targetPosition.x, t), Mathf.SmoothStep(selector.position.y, targetPosition.y, t), targetPosition.z);
            if (selector.localScale.x != targetScale.x)
            {
                selector.localScale = new Vector3(Mathf.SmoothStep(selector.localScale.x, targetScale.x, t), Mathf.SmoothStep(selector.localScale.y, targetScale.y, t), 1f);
            }
            yield return new WaitForFixedUpdate();
            if (!endLoop && selector.position == targetPosition && (Vector2)selector.localScale == targetScale)
            {
                endLoop = true;
                LoseLife?.Invoke();
                yield return new WaitForSeconds(2f);
                Unactive?.Invoke();
                break;
            }
        }
        yield break;
    }



    public IEnumerator SmoothMoveDrawer(Transform selector, Vector3 targetPosition, float movementDuration, Action beginAction, Action endAction)
    {
        beginAction?.Invoke();
        float startTime = Time.time;
        float t;
        while (selector.position.x != targetPosition.x)
        {
            t = (Time.time - startTime) / movementDuration;
            selector.position = new Vector3(Mathf.SmoothStep(selector.position.x, targetPosition.x, t), Mathf.SmoothStep(selector.position.y, targetPosition.y, t), targetPosition.z);

            if (selector.position.x == targetPosition.x)
            {
                endAction?.Invoke();
                break;
            }
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }



    public IEnumerator SmoothMoveRank(Transform selector, float movementDuration, Action endActionEmptyScroll, Action endActionFullScroll, Action finishSliding)
    {
        float startTime = Time.time;
        float t;
        float targetPositionX;
        bool toFull;
        float targetFull = 2.61f;
        float targetEmpty = 14.72f;

        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.OpenDrawer, false);
        if (selector.localPosition.x <= targetFull)
        {
            toFull = false;
            targetPositionX = targetEmpty;
        }
        else
        {
            toFull = true;
            targetPositionX = targetFull;
        }
        if (toFull)
        {
            endActionFullScroll?.Invoke();
        }
        while (selector.localPosition.x != targetPositionX)
        {
            t = (Time.time - startTime) / movementDuration;
            selector.localPosition = new Vector2(Mathf.SmoothStep(selector.localPosition.x, targetPositionX, t / 2), selector.localPosition.y);

            if (selector.localPosition.x == targetPositionX)
            {
                finishSliding?.Invoke();
                if (!toFull)
                {
                    endActionEmptyScroll?.Invoke();
                }
            }
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    public IEnumerator ShowDialogFromSide(GameObject dialog, bool isEnable, Action EndAction)
    {
        Transform selector = dialog.transform;
        float startTime = Time.time;
        float t;
        float targetX = -7.7f;
        float startingPos = -12f;
        float movementDuration = 0.5f;

        if (!isEnable)
        {
            targetX = startingPos;
        }
        else
        {
            dialog.SetActive(true);
        }
        Vector2 targetPosition = new Vector2(targetX, selector.localPosition.y);
        while ((Vector2)selector.localPosition != targetPosition)
        {
            t = (Time.time - startTime) / movementDuration;
            yield return new WaitForFixedUpdate();
            selector.localPosition = new Vector2(Mathf.SmoothStep(selector.localPosition.x, targetPosition.x, t / 2), selector.localPosition.y);
            if ((Vector2)selector.localPosition == targetPosition)
            {
                EndAction?.Invoke();
                if (targetX == startingPos)
                {
                    dialog.SetActive(false);
                }
                yield break;
            }
        }
        yield break;
    }
    public IEnumerator ShowDialogFromPu(Transform selector, SpriteRenderer dialogSprite, Vector2 startingPosition, Vector2 targetTransform, Action EndAction)
    {
        float startTime = Time.time;
        float t;
        float interval = 1.5f;
        float movementDuration = Values.Instance.showDialogMoveDuration;
        Vector2 targetScale = new Vector2(1, 1);
        SetAlpha(dialogSprite, 1f);

        while ((Vector2)selector.position != targetTransform || (Vector2)selector.localScale != targetScale)
        {

            t = (Time.time - startTime) / movementDuration;
            yield return new WaitForFixedUpdate();
            selector.position = new Vector2(Mathf.SmoothStep(startingPosition.x, targetTransform.x, t * interval), Mathf.SmoothStep(startingPosition.y, targetTransform.y, t * interval));
            selector.localScale = new Vector2(Mathf.SmoothStep(selector.localScale.x, targetScale.x, t * interval), Mathf.SmoothStep(selector.localScale.y, targetScale.y, t * interval));
            if ((Vector2)selector.position == targetTransform && (Vector2)selector.localScale == targetScale)
            {
                EndAction?.Invoke();
                yield break;
            }
        }
        yield break;
    }
    public IEnumerator SpinRotateValue(SpriteRenderer projectileSpriteRen, Action EndAction)
    {
        float startTime = Time.time;
        float t = 0;
        float value = 0;
        SetAlpha(projectileSpriteRen, 1f);
        projectileSpriteRen.material.SetFloat("_RotateUvAmount", 0);
        float totalTime = 2f;
        while (t < totalTime)
        {
            t = (Time.time - startTime) / totalTime;
            value += Time.time;
            projectileSpriteRen.material.SetFloat("_RotateUvAmount", value * 5);
            if (value >= 6.28f)
            {
                value = 0;
            }
            if (t >= totalTime)
            {
                EndAction?.Invoke();
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
    public IEnumerator HitEffect(SpriteRenderer hitSpriteRen, Action EndAction)
    {
        /*float startTime = Time.time;
        float t = 0 ;
        float value = 0 ;
       // SetAlpha(hitSpriteRen, 1f);
        hitSpriteRen.material.SetFloat("_HitEffectBlend", 0);
        yield return new WaitForSeconds(2f);

        while (value<1)
        {
            Debug.LogError("hit " + hitSpriteRen.material.GetFloat("_HitEffectBlend"));
            yield return new WaitForFixedUpdate();
            value += Time.time;
            hitSpriteRen.material.SetFloat("_HitEffectBlend", value);
            if (value >= 1)
            {
                hitSpriteRen.material.SetFloat("_HitEffectBlend", 0);
                yield return new WaitForSeconds(0.8f);
                EndAction?.Invoke();
                yield break;
            }
        Debug.LogError("Hit " + value + " t " + t);
        }
        yield break;*/
        yield break;

    }

    public IEnumerator FlipCard(Transform selector, float flipDuration, Action updateBoolDone, Action flip, Action endAction)
    {
        float startTime = Time.time;
        float t;
        bool shrinking = true;
        Vector3 vectorTargetShrink = new Vector3(0.01f, selector.localScale.y, 1f);
        Vector3 vectorTargetFinal = new Vector3(selector.localScale.x, selector.localScale.y, 1f);
        Vector3 target = vectorTargetShrink;
        selector.localScale = new Vector3(selector.localScale.x - 0.01f, selector.localScale.y, 1f);
        while (selector.localScale != vectorTargetFinal)
        {
            t = (Time.time - startTime) / flipDuration;
            if (!shrinking)
            {
                if (selector.localScale == vectorTargetShrink)
                {
                    endAction?.Invoke();
                    updateBoolDone?.Invoke();
                }

            }
            selector.localScale = new Vector3(Mathf.SmoothStep(selector.localScale.x, target.x, t), selector.localScale.y, 1f);
            if (selector.localScale == vectorTargetShrink)
            {
                flip?.Invoke();
                shrinking = false;
                target = vectorTargetFinal;
                startTime = Time.time;
            }
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    public IEnumerator SpinCoin(Transform selector, float flipDuration, Action endAction)
    {
        float startTime = Time.time;
        float t;
        bool shrinking = true;
        int totalSpinCounter = 6;
        float target = 0.01f;
        selector.localScale = new Vector2(selector.localScale.x - 0.01f, selector.localScale.y);
        while (totalSpinCounter > 0)
        {
            t = (Time.time - startTime) / flipDuration;
            if (shrinking && selector.localScale.x <= 0.01f)
            {
                t = 0;
                shrinking = false;
                target = 1f;
                startTime = Time.time;
                yield return new WaitForSeconds(0.01f);
            }
            selector.localScale = new Vector2(Mathf.SmoothStep(selector.localScale.x, target, t), selector.localScale.y);
            if (!shrinking && selector.localScale.x >= 1f)
            {
                shrinking = true;
                target = 0.01f;
                startTime = Time.time;
                totalSpinCounter--;
                yield return new WaitForSeconds(0.01f);
                flipDuration -= 0.08f;
            }

            yield return new WaitForFixedUpdate();
        }
        yield break;
    }
    public IEnumerator ScaleObjectWheel(Transform selector, float target, float flipDuration)
    {
        float startTime = Time.time;
        float t;
        while (selector.localScale.x != target)
        {
            t = (Time.time - startTime) / flipDuration;
            selector.localScale = new Vector2(Mathf.SmoothStep(selector.localScale.x, target, t), Mathf.SmoothStep(selector.localScale.y, target, t));
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    public IEnumerator ScaleObject(bool scaleDown, float scaleTarget, float scaleDuration, Transform selector, Action RedBgEnable, Action onEnd)
    {
        float startTime = Time.time;
        float t;
        bool endActivate = true;
        Vector3 vectorTarget = new Vector3(selector.localScale.x * scaleTarget, selector.localScale.y * scaleTarget, selector.localScale.z);
        Vector3 vectorOriginal = new Vector3(selector.localScale.x, selector.localScale.y, selector.localScale.z);
        while (selector.localScale.x != vectorTarget.x)
        {
            t = (Time.time - startTime) / scaleDuration;
            if (scaleDown)
            {
                selector.localScale = new Vector3(Mathf.SmoothStep(vectorTarget.x, selector.localScale.x, t), Mathf.SmoothStep(vectorTarget.y, selector.localScale.y, t), vectorTarget.z);
            }
            else
            {
                selector.localScale = new Vector3(Mathf.SmoothStep(selector.localScale.x, vectorTarget.x, t), Mathf.SmoothStep(selector.localScale.y, vectorTarget.y, t), vectorTarget.z);
                /* if (activate && selector.localScale.x > vectorTarget.x/2)
                 {

                     activate = false;
                     AlphaAnimation?.Invoke();
                 }*/

            }
            yield return new WaitForFixedUpdate();
            if (endActivate && selector.localScale.x == vectorTarget.x)
            {
                endActivate = false;
                RedBgEnable?.Invoke();
                onEnd?.Invoke();
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public IEnumerator AnimateShootProjectile(bool activateFade, Transform projectile, Vector3 posTarget, Action FadeOut, Action endAction)
    {

        float startTime = Time.time;
        float t;
        float speed = 3f;
        float movementDuration = Values.Instance.puProjectileMoveDuration;
        while (projectile.position != posTarget)
        {

            t = (Time.time - startTime) / movementDuration;

            projectile.position = new Vector3(Mathf.SmoothStep(projectile.position.x, posTarget.x, t * speed), Mathf.SmoothStep(projectile.position.y, posTarget.y, t * speed), projectile.position.z);
            yield return new WaitForFixedUpdate();
            if (activateFade/* && t > 0.5f*/ )
            {
                float dist = Vector3.Distance(projectile.position, posTarget);
                if (dist < 1.5f)
                {

                    activateFade = false;
                    FadeOut?.Invoke();
                    endAction?.Invoke();
                }
            }
            else if (projectile.position == posTarget)
            {
                FadeOut?.Invoke();
                endAction?.Invoke();
            }
        }
        if (projectile.position == posTarget)
        {


        }
    }

    public IEnumerator AnimateWind(SpriteRenderer windSpriteRenderer, Action PuIgnite, Action FadeOut)
    {
        SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.WindSound);
        float windMoveDuration = Values.Instance.windMoveDuration;

        bool activatePu = true;
        float zoomAmount = 5f;
        float rotateAmount = 6.2831f;
        SetAlpha(windSpriteRenderer, 1f);
        windSpriteRenderer.material.SetFloat("_ZoomUvAmount", zoomAmount);
        windSpriteRenderer.material.SetFloat("_RotateUvAmount", rotateAmount);
        while (zoomAmount > 0)
        {
            zoomAmount -= Time.deltaTime / windMoveDuration;
            rotateAmount -= Time.deltaTime / windMoveDuration;
            windSpriteRenderer.material.SetFloat("_ZoomUvAmount", zoomAmount);
            windSpriteRenderer.material.SetFloat("_RotateUvAmount", rotateAmount);
            yield return new WaitForFixedUpdate();
            if (activatePu && zoomAmount < 4)
            {
                activatePu = false;
                PuIgnite?.Invoke();

            }
            if (zoomAmount <= 0.1)
            {
                FadeOut?.Invoke();
                break;
            }
        }
    }

    public void SetAlpha(SpriteRenderer windSpriteRenderer, float alpha)
    {
        windSpriteRenderer.color = new Color(windSpriteRenderer.material.color.r, windSpriteRenderer.material.color.g, windSpriteRenderer.material.color.b, alpha);
    }

    public IEnumerator ScaleAndFadeEye(Transform selector, bool disableImage, Action ImateReplace, Action disableDarkScreen)
    {

        Vector2 targetScale;
        if (disableImage)
        {
            targetScale = new Vector3(4f, 4f, 1f);
        }
        else
        {
            targetScale = new Vector3(0.76f, 0.76f, 1f);

        }
        SpriteRenderer spriteRenderer = selector.GetComponent<SpriteRenderer>();
        float startTime = Time.time;
        float movementDuration = Values.Instance.markOnCardScaleAlphaDuration;
        float t;
        float alpha;
        float counter = 0;

        while ((Vector2)selector.localScale != targetScale)

        {
            t = (Time.time - startTime) / movementDuration;
            counter += Time.time;
            if (disableImage)
            {
                alpha = Mathf.Lerp(1, 0, t * 2);
            }
            else
            {
                alpha = Mathf.Lerp(0, 1, t * 2);
            }
            spriteRenderer.material.color = new Color(1, 1, 1, alpha);
            selector.localScale = new Vector2(Mathf.SmoothStep(selector.localScale.x, targetScale.x, t), Mathf.SmoothStep(selector.localScale.y, targetScale.y, t));
            yield return new WaitForFixedUpdate();
            if ((Vector2)selector.localScale == targetScale)
            {
                disableDarkScreen?.Invoke();
                if (disableImage)
                {
                    spriteRenderer.material.color = new Color(1, 1, 1, 0);
                }
                yield return new WaitForFixedUpdate();
                ImateReplace?.Invoke();
                break;
            }
        }
        yield break;
    }

    internal IEnumerator FadeCanvasGroup(CanvasGroup emojisWheel, bool enable, float duration)
    {
        float targetAlpha = 0f;
        float alphaAmount = 1f;
        if (enable)
        {
            targetAlpha = 1f;
            alphaAmount = 0f;
        }
        while (emojisWheel.alpha != targetAlpha)
        {
            if (enable)
            {
                alphaAmount += Time.deltaTime / duration;
            }
            else
            {
                alphaAmount -= Time.deltaTime / duration;
            }
            yield return new WaitForFixedUpdate();
            emojisWheel.alpha = alphaAmount;
            if (alphaAmount > 1f || alphaAmount < 0)
            {
                emojisWheel.alpha = targetAlpha;
                break;
            }
        }

    }

    public void VisionEffect(List<CardUi> winningPlayersCards, bool enable)
    {
        float alphaAmoint = 0f;
        float burnAmoint = 0.6f;
        Color visionColor = Values.Instance.currentVisionColor;
        Color ghostColor = Values.Instance.ghostOutlineColor;
        if (enable)
        {
            alphaAmoint = 1f;
            burnAmoint = 0f;
            ghostColor = visionColor;
        }
        foreach (CardUi cardUi in winningPlayersCards)
        {
            if (cardUi.freeze)
            {
                cardUi.spriteRenderer.material.SetFloat("_FadeAmount", burnAmoint);
            }
            if (cardUi.isGhost)
            {
                cardUi.spriteRenderer.material.SetColor("_OutlineColor", ghostColor);
            }
            else
            {
                cardUi.spriteRenderer.material.SetColor("_OutlineColor", visionColor);
                cardUi.spriteRenderer.material.SetFloat("_OutlineAlpha", alphaAmoint);
            }
        }
    }



    public async void AnimateWinningHandToBoard(List<CardUi> winningPlayerCards, List<CardUi> losingBoardCards, Action UpdateValueEndRoutine)
    {
        await Task.Delay(500);

        //int layoutOrder = losingBoardCards[0].spriteRenderer.sortingOrder;
        for (int i = 0; i < winningPlayerCards.Count; i++)
        {
            //winningPlayerCards[i].EnableSelecetPositionZ(true);
            Vector3 targetPosition = new Vector3(losingBoardCards[i].transform.position.x, losingBoardCards[i].transform.position.y, winningPlayerCards[i].transform.position.z);
            StartCoroutine(SmoothMove(winningPlayerCards[i].transform, targetPosition, losingBoardCards[i].transform.localScale, Values.Instance.winningCardsMoveDuration, null, UpdateValueEndRoutine, null, null));
        }
        foreach(CardUi card in losingBoardCards)
        {
            StartCoroutine(card.FadeBurnOut(card.spriteRenderer.material, false, null));
        }
    }




    public IEnumerator FadeDarkScreen(Material targetObj, bool fadeIn, float duration, Action onFinishDissolve3)
    {
        float tiling = UnityEngine.Random.Range(0.07f, 0.15f);
        //  float tiling = UnityEngine.Random.Range(0.07f, 0.15f);
        // targetObj.SetTextureScale("_FadeTex", new Vector2(tiling, tiling));
        // targetObj.SetTextureOffset("_FadeTex", new Vector2(tiling, tiling));
        float dissolveAmount = -0.1f;

        if (fadeIn)
        {
            dissolveAmount = 1f;
        }
        while (dissolveAmount >= -0.1f && dissolveAmount <= 1f)
        {
            if (fadeIn)
            {
                dissolveAmount -= Time.deltaTime / duration;
            }
            else
            {
                dissolveAmount += Time.deltaTime / duration;
            }
            targetObj.SetFloat("_FadeAmount", dissolveAmount);
            yield return new WaitForFixedUpdate();
            if (dissolveAmount < -0.1f || dissolveAmount > 1f)
            {

                yield return new WaitForSeconds(0.1f);
                onFinishDissolve3?.Invoke();
                break;
            }
        }
    }
    public IEnumerator FadeBurnPU(Material targetObj, float delayBefore, bool fadeIn, float duration, Action onFinishDissolve, Action onFinishDissolve2, Action onFinishDissolve3)
    {
        // float tiling = UnityEngine.Random.Range(0.07f, 0.15f);
        //  float tiling = UnityEngine.Random.Range(0.07f, 0.15f);
        // targetObj.SetTextureScale("_FadeTex", new Vector2(tiling, tiling));
        // targetObj.SetTextureOffset("_FadeTex", new Vector2(tiling, tiling));
        float dissolveAmount = -0.1f;

        yield return new WaitForSeconds(delayBefore);
        if (fadeIn)
        {
            dissolveAmount = 1f;
        }
        while (dissolveAmount >= -0.1f && dissolveAmount <= 1f)
        {
            if (fadeIn)
            {
                dissolveAmount -= Time.deltaTime / duration;
            }
            else
            {
                dissolveAmount += Time.deltaTime / duration;
            }
            targetObj.SetFloat("_FadeAmount", dissolveAmount);
            yield return new WaitForFixedUpdate();
            if (dissolveAmount < -0.1f || dissolveAmount > 1f)
            {
                onFinishDissolve?.Invoke();
                yield return new WaitForSeconds(0.1f);
                onFinishDissolve2?.Invoke();
                yield return new WaitForSeconds(0.5f);
                onFinishDissolve3?.Invoke();
                break;
            }
        }
    }

    public IEnumerator FadeEnergy(Action pulse, float delay, Material targetObj, bool fadeIn, float duration, Action onFinishDissolve3)
    {
        pulse?.Invoke();
        yield return new WaitForSeconds(delay);
        float dissolveAmount = -0.1f;
        if (fadeIn)
        {
            dissolveAmount = 1f;
        }
        while (dissolveAmount >= -0.1f && dissolveAmount <= 1f)
        {
            if (fadeIn)
            {
                dissolveAmount -= Time.deltaTime / duration;
            }
            else
            {
                dissolveAmount += Time.deltaTime / duration;
            }
            targetObj.SetFloat("_FadeAmount", (Mathf.SmoothStep(-0.1f, 1f, dissolveAmount)));
            yield return new WaitForFixedUpdate();
            if (dissolveAmount < -0.1f || dissolveAmount > 1f)
            {
                onFinishDissolve3?.Invoke();
                break;
            }
        }

    }


    public IEnumerator FadeColorSwapBlend(SpriteRenderer targetObj, bool increas, float target, float duration)
    {
        float dissolveAmount = targetObj.material.GetFloat("_ColorSwapBlend");

        while (dissolveAmount != target)
        {
            Debug.LogError("walla" + dissolveAmount);
            yield return new WaitForFixedUpdate();
            if (increas)
            {
                dissolveAmount += Time.deltaTime / duration;
            }
            else
            {
                dissolveAmount -= Time.deltaTime / duration;
            }
            if (dissolveAmount < 0 || dissolveAmount > target)
            {
                targetObj.material.SetFloat("_ColorSwapBlend", target);
                break;
            }
            targetObj.material.SetFloat("_ColorSwapBlend", dissolveAmount);
        }
    }

    public IEnumerator PulseColorAnimation(SpriteRenderer targetObj, bool enable, float duration)
    {
        yield return new WaitForSeconds(0.5f);
        bgAnimation = enable;
        float maxValue = Values.Instance.bgMaxValueSwapColor;
        float dissolveAmount = targetObj.material.GetFloat("_ColorSwapBlend");
        bool floatUp = true;
        if (bgAnimation && dissolveAmount == 0)
        {

            while (bgAnimation)
            {
                if (!bgAnimation)
                {
                    yield return new WaitForFixedUpdate();

                    dissolveAmount -= Time.deltaTime / duration;
                    // value = Mathf.SmoothStep(0f, 0.5f, dissolveAmount );
                    if (dissolveAmount < 0)
                    {
                        targetObj.material.SetFloat("_ColorSwapBlend", 0);
                        dissolveAmount = 0;
                    }
                    targetObj.material.SetFloat("_ColorSwapBlend", dissolveAmount);
                    break;
                }

                if (floatUp)
                {
                    dissolveAmount += Time.deltaTime / duration;
                }
                else
                {
                    dissolveAmount -= Time.deltaTime / duration;
                }
                if (dissolveAmount >= maxValue)
                {
                    yield return new WaitForSeconds(0.11f);
                    floatUp = false;
                }
                else if (dissolveAmount <= 0f)
                {
                    yield return new WaitForSeconds(0.01f);
                    floatUp = true;
                }
                //    value = Mathf.SmoothStep(0.5f, 0.75f, dissolveAmount);
                targetObj.material.SetFloat("_ColorSwapBlend", dissolveAmount);

                yield return new WaitForFixedUpdate();

            }
            /*  if (!bgAnimation)
              {
                      Debug.LogError("IMOUT2");
                  targetObj.material.SetFloat("_ColorSwapBlend", 0f);
              }*/
        }
        else
        {
            while (dissolveAmount > 0f)
            {
                //targetObj.material.SetFloat("_ColorSwapBlend", 5f);


                yield return new WaitForFixedUpdate();

                dissolveAmount -= Time.deltaTime / duration;
                // value = Mathf.SmoothStep(0f, 0.5f, dissolveAmount );
                if (dissolveAmount < 0)
                {
                    targetObj.material.SetFloat("_ColorSwapBlend", 0);
                    dissolveAmount = 0;
                }
                targetObj.material.SetFloat("_ColorSwapBlend", dissolveAmount);


            }
        }

    }

    /*public static async void OutlineInit(Material targetObj, bool enable)
    {

        float outlineAlpha;
        if (enable)
        {
            stopOutlinePulse = false;
            outlineAlpha = 0.2f;
            instance.StartCoroutine(OutlinePulseAnimation(targetObj));
        }
        else
        {
            outlineAlpha = 0.0f;
            stopOutlinePulse = true;

        }
        Debug.LogError("OL: " + targetObj.name + " " + enable);
        targetObj.SetFloat("_OutlineAlpha", outlineAlpha);
        await Task.CompletedTask;
    }

    private static IEnumerator OutlinePulseAnimation(Material targetObj)
    {
        float dissolveAmount = 0.2f;
        bool floatUp = true;
        while (!stopOutlinePulse)
        {
            //Debug.LogError("Running");

            if (floatUp)
            {
                dissolveAmount += Time.deltaTime;
            }
            else
            {
                dissolveAmount -= Time.deltaTime;
            }
            if (dissolveAmount >= 1f)
            {
                yield return new WaitForSeconds(0.2f);
                floatUp = false;
            }
            else if (dissolveAmount <= 0.2f)
            {
                floatUp = true;
            }
            targetObj.SetFloat("_OutlineAlpha", Mathf.SmoothStep(0.2f, 1f, dissolveAmount));
            //  targetObj.SetFloat("_OutlineAlpha", dissolveAmount);
            yield return new WaitForFixedUpdate();
            if (stopOutlinePulse)
            {
                targetObj.SetFloat("_OutlineAlpha", 0f);
                break;
            }
        }
    }
*/
    /* public static IEnumerator SmoothMovePuToCenter(Transform selector, Vector2 targetPosition, Vector2 targetScale, float movementDuration, Action beginAction, Action endAction)
     {
         SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.PuUse);

         float startTime = Time.time;
         float t = 0;
         //beginAction?.Invoke();
         // while ((Vector2)selector.position != targetPosition)
         while ((Vector2)selector.position != targetPosition)
         {
             //  Debug.LogError("Running");

             t = (Time.time - startTime) / movementDuration;
             selector.position = new Vector2(Mathf.SmoothStep(selector.position.x, targetPosition.x, t), Mathf.SmoothStep(selector.position.y, targetPosition.y, t));
             selector.localScale = new Vector2(Mathf.SmoothStep(selector.localScale.x, targetScale.x, t), Mathf.SmoothStep(selector.localScale.y, targetScale.y, t));
             if ((Vector2)selector.position == targetPosition)
             {
                 Debug.LogError("Position Finished: " + selector.position.x + " y " + selector.position.y);

                 endAction?.Invoke();
                 break;
             }
             yield return new WaitForFixedUpdate();
         }
         yield break;
     }*/

    /* internal static void GlowCards(List<CardUi> winningPlayersCards, bool enable)
  {

      foreach (CardUi cardUi in winningPlayersCards)
      {
          cardUi.SetCardFrame(false);
          cardUi.cardSelection.SetActive(enable);
      }
  }  */

    /*public static IEnumerator SmoothMoveEyes(Transform selector, float movementDuration, Action beginAction, Action endAction)
    {
        float startTime = Time.time;
        float t;
        bool activateSlash = true;
        beginAction?.Invoke();
        Vector2 targetPosition = new Vector2(-8f, selector.position.y);
        while ((Vector2)selector.position != targetPosition)
        {
            t = (Time.time - startTime) / movementDuration;
            selector.position = new Vector2(Mathf.SmoothStep(selector.position.x, targetPosition.x, t / 2), selector.position.y);
            if (activateSlash && selector.position.x < -4)
            {
                activateSlash = false;
                endAction?.Invoke();
            }
            if ((Vector2)selector.position == targetPosition)
            {
            }
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }*/

}
