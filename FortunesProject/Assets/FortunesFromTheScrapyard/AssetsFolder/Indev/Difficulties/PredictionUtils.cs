using System;
using System.Reflection;
using FortunesFromTheScrapyard;
using EntityStates;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using RoR2.Projectile;
using UnityEngine;
using MSU;
using MSU.Config;

namespace FortunesFromTheScrapyard
{
    public static class PredictionUtils
    {
        private const float zero = 0.00001f;

        /// <summary>
        /// aimRay must be on the stack before calling this!
        /// </summary>
        /// <param name="c"></param>
        /// <param name="type"></param>
        public static void EmitPredictAimray<T>(this ILCursor c, string prefabName = "projectilePrefab")
        {
            //this.characterbody
            c.Emit(OpCodes.Ldarg_0);
            c.Emit(OpCodes.Call, typeof(EntityState).GetProperty("get_characterBody"));

            // this.projectilePrefab
            // - or -
            // {TYPE}.projectilePrefab
            var fieldInfo = typeof(T).GetField(prefabName, (BindingFlags)(-1));
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldarg_0);
            if (!fieldInfo.IsStatic) c.Emit(OpCodes.Ldfld, fieldInfo);
            else c.Emit(OpCodes.Ldsfld, fieldInfo);

            // Utils.PredictAimRay(aimRay, characterBody, projectilePrefab);
            c.Emit(OpCodes.Call, typeof(PredictionUtils).GetMethodCached(nameof(PredictAimray)));
        }

        public static Ray PredictAimray(Ray aimRay, CharacterBody body, GameObject projectilePrefab)
        {
            if (!body || !body.master || !projectilePrefab)
                return aimRay;

            var projectileSpeed = 0f;
            if (projectilePrefab.TryGetComponent<ProjectileSimple>(out var ps))
                projectileSpeed = ps.desiredForwardSpeed;

            if (projectilePrefab.TryGetComponent<ProjectileCharacterController>(out var pcc))
                projectileSpeed = Mathf.Max(projectileSpeed, pcc.velocity);

            var targetBody = GetAimTargetBody(body);
            if (projectileSpeed > 0f && targetBody)
            {
                //Velocity shows up as 0 for clients due to not having authority over the CharacterMotor
                //Less accurate, but it works online.
                Vector3 vT, aT, pT = targetBody.transform.position;
                if (targetBody.characterMotor && targetBody.characterMotor.hasEffectiveAuthority)
                {
                    vT = targetBody.characterMotor.velocity;
                    aT = Vector3.zero;
                }
                else
                {
                    vT = (pT - targetBody.previousPosition) / Time.fixedDeltaTime;
                    aT = Vector3.zero;
                }

                if (vT.sqrMagnitude > zero) //Dont bother predicting stationary targets
                {
                    return GetRay(aimRay, projectileSpeed, pT, vT, aT);
                }
            }

            return aimRay;
        }

        //All in world space! Gets point you have to aim to
        //NOTE: this will break with infinite speed projectiles!
        //https://gamedev.stackexchange.com/questions/149327/projectile-aim-prediction-with-acceleration
        public static Ray GetRay(Ray aimRay, float sP, Vector3 pT, Vector3 vT, Vector3 aT)
        {
            //time to target guess
            var t = Vector3.Distance(aimRay.origin, pT) / sP;

            // target position relative to ray position
            pT -= aimRay.origin;

            var useAccel = aT.sqrMagnitude > zero;

            //quartic coefficients
            // a = t^4 * (aT·aT / 4.0)
            // b = t^3 * (aT·vT)
            // c = t^2 * (aT·pT + vT·vT - s^2)
            // d = t   * (2.0 * vT·pT)
            // e =       pT·pT
            var c = vT.sqrMagnitude - Pow2(sP);
            var d = 2f * Vector3.Dot(vT, pT);
            var e = pT.sqrMagnitude;

            if (useAccel)
            {
                var a = aT.sqrMagnitude * 0.25f;
                var b = Vector3.Dot(aT, vT);
                c += Vector3.Dot(aT, pT);

                //solve with newton
                t = SolveQuarticNewton(t, 6, a, b, c, d, e);
            }
            else
            {
                t = SolveQuadraticNewton(t, 6, c, d, e);
            }

            if (t > 0f)
            {
                //p(t) = pT + (vT * t) + ((aT/2.0) * t^2)
                var relativeDest = pT + (vT * t);
                if (useAccel)
                    relativeDest += 0.5f * aT * Pow2(t);

                return new Ray(aimRay.origin, relativeDest);
            }
            return aimRay;

        }

        private static float SolveQuarticNewton(float guess, int iterations, float a, float b, float c, float d, float e)
        {
            for (var i = 0; i < iterations; i++)
            {
                guess -= EvalQuartic(guess, a, b, c, d, e) / EvalQuarticDerivative(guess, a, b, c, d);
            }
            return guess;
        }

        private static float EvalQuartic(float t, float a, float b, float c, float d, float e)
        {
            return (a * Pow4(t)) + (b * Pow3(t)) + (c * Pow2(t)) + (d * t) + e;
        }

        private static float EvalQuarticDerivative(float t, float a, float b, float c, float d)
        {
            return (4f * a * Pow3(t)) + (3f * b * Pow2(t)) + (2f * c * t) + d;
        }

        private static float SolveQuadraticNewton(float guess, int iterations, float a, float b, float c)
        {
            for (var i = 0; i < iterations; i++)
            {
                guess -= EvalQuadratic(guess, a, b, c) / EvalQuadraticDerivative(guess, a, b);
            }
            return guess;
        }

        private static float EvalQuadratic(float t, float a, float b, float c)
        {
            return (a * Pow2(t)) + (b * t) + c;
        }

        private static float EvalQuadraticDerivative(float t, float a, float b)
        {
            return (2f * a * t) + b;
        }

        private static float Pow2(float n) => n * n;
        private static float Pow3(float n) => n * n * n;
        private static float Pow4(float n) => n * n * n * n;

        private static CharacterBody GetAimTargetBody(CharacterBody body)
        {
            var aiComponents = body.master.aiComponents;
            for (var i = 0; i < aiComponents.Length; i++)
            {
                var ai = aiComponents[i];
                if (ai && ai.hasAimTarget)
                {
                    var aimTarget = ai.skillDriverEvaluation.aimTarget;
                    if (aimTarget.characterBody && aimTarget.healthComponent && aimTarget.healthComponent.alive)
                    {
                        return aimTarget.characterBody;
                    }
                }
            }
            return null;
        }
    }
}
