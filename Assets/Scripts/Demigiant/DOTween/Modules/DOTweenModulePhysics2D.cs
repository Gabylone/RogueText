﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2018/07/13

#if true && (UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_2017_1_OR_NEWER) // MODULE_MARKER
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;

namespace DG.Tweening {
    public static class DOTweenModulePhysics2D {
        #region Shortcuts

        #region Rigidbody2D Shortcuts

        /// <summary>Tweens a Rigidbody2D's position to the given value.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations</summary>
        /// <getParam name="endValue">The end value to reach</getParam><getParam name="duration">The duration of the tween</getParam>
        /// <getParam name="snapping">If TRUE the tween will smoothly snap all values to integers</getParam>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOMove(this Rigidbody2D target, Vector2 endValue, float duration, bool snapping = false) {
            var t = DOTween.To(() => target.position, target.MovePosition, endValue, duration);
            _ = t.SetOptions(snapping).SetTarget(target);
            return t;
        }

        /// <summary>Tweens a Rigidbody2D's X position to the given value.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations</summary>
        /// <getParam name="endValue">The end value to reach</getParam><getParam name="duration">The duration of the tween</getParam>
        /// <getParam name="snapping">If TRUE the tween will smoothly snap all values to integers</getParam>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOMoveX(this Rigidbody2D target, float endValue, float duration, bool snapping = false) {
            var t = DOTween.To(() => target.position, target.MovePosition, new Vector2(endValue, 0), duration);
            _ = t.SetOptions(AxisConstraint.X, snapping).SetTarget(target);
            return t;
        }

        /// <summary>Tweens a Rigidbody2D's Y position to the given value.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations</summary>
        /// <getParam name="endValue">The end value to reach</getParam><getParam name="duration">The duration of the tween</getParam>
        /// <getParam name="snapping">If TRUE the tween will smoothly snap all values to integers</getParam>
        public static TweenerCore<Vector2, Vector2, VectorOptions> DOMoveY(this Rigidbody2D target, float endValue, float duration, bool snapping = false) {
            var t = DOTween.To(() => target.position, target.MovePosition, new Vector2(0, endValue), duration);
            _ = t.SetOptions(AxisConstraint.Y, snapping).SetTarget(target);
            return t;
        }

        /// <summary>Tweens a Rigidbody2D's rotation to the given value.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations</summary>
        /// <getParam name="endValue">The end value to reach</getParam><getParam name="duration">The duration of the tween</getParam>
        public static TweenerCore<float, float, FloatOptions> DORotate(this Rigidbody2D target, float endValue, float duration) {
            var t = DOTween.To(() => target.rotation, target.MoveRotation, endValue, duration);
            _ = t.SetTarget(target);
            return t;
        }

        #region Special

        /// <summary>Tweens a Rigidbody2D's position to the given value, while also applying a jump effect along the Y axis.
        /// Returns a Sequence instead of a Tweener.
        /// Also stores the Rigidbody2D as the tween's target so it can be used for filtered operations.
        /// <para>IMPORTANT: a rigidbody2D can't be animated in a jump arc using MovePosition, so the tween will directly set the position</para></summary>
        /// <getParam name="endValue">The end value to reach</getParam>
        /// <getParam name="jumpPower">Power of the jump (the max height of the jump is represented by this plus the final Y offset)</getParam>
        /// <getParam name="numJumps">Total number of jumps</getParam>
        /// <getParam name="duration">The duration of the tween</getParam>
        /// <getParam name="snapping">If TRUE the tween will smoothly snap all values to integers</getParam>
        public static Sequence DOJump(this Rigidbody2D target, Vector2 endValue, float jumpPower, int numJumps, float duration, bool snapping = false) {
            if (numJumps < 1) numJumps = 1;
            float startPosY = 0;
            float offsetY = -1;
            var offsetYSet = false;
            var s = DOTween.Sequence();
            Tween yTween = DOTween.To(() => target.position, x => target.position = x, new Vector2(0, jumpPower), duration / (numJumps * 2))
                .SetOptions(AxisConstraint.Y, snapping).SetEase(Ease.OutQuad).SetRelative()
                .SetLoops(numJumps * 2, LoopType.Yoyo)
                .OnStart(() => startPosY = target.position.y);
            _ = s.Append(DOTween.To(() => target.position, x => target.position = x, new Vector2(endValue.x, 0), duration)
                    .SetOptions(AxisConstraint.X, snapping).SetEase(Ease.Linear)
                ).Join(yTween)
                .SetTarget(target).SetEase(DOTween.defaultEaseType);
            _ = yTween.OnUpdate(() => {
                if (!offsetYSet) {
                    offsetYSet = true;
                    offsetY = s.isRelative ? endValue.y : endValue.y - startPosY;
                }
                Vector3 pos = target.position;
                pos.y += DOVirtual.EasedValue(0, offsetY, yTween.ElapsedPercentage(), Ease.OutQuad);
                target.MovePosition(pos);
            });
            return s;
        }

        #endregion

        #endregion

        #endregion
    }
}
#endif
