using System;
using System.Collections;
using System.Collections.Generic;
using Message;
using UnityEngine;


namespace Sense
{
    /// <summary>
    /// All sense types even the magical ones should be defined here.
    /// </summary>
    public enum SenseType
    {
        None,
        Vision,
        Hearing,
    }

    /// <summary>
    /// Base class for all senses.
    /// </summary>
    public abstract class AI_SenseBase : Entity
    {
        // the type of the sense
        // hide this in inspector, you probably do not want someone to mark say a Vision sense
        // to hearing type.
        // Sense type is None for now, hard code it to desired type in subclasses.
        [HideInInspector]
        public SenseType senseType = SenseType.None;
    }


    public class FOV : AI_SenseBase
    {
        public float viewRadius;
        public float delay = 0.1f;

        [Range(0, 360)]
        public float viewAngle;

        public LayerMask targetMask;
        public LayerMask obstacleMask;
        public LayerMask itemMask;

        // AI targets that are directly visible in FOV cove
        public List<Transform> visibleTargets = new List<Transform>();
        public List<Transform> VisibleTargets { get { return visibleTargets; } set { visibleTargets = value; } }

        // AI targets that are directly visible view radius
        public List<Transform> targetsInViewRadius = new List<Transform>();
        public List<Transform> TargetsInViewRadius { get { return targetsInViewRadius; } }

        // items that are directly visible
        //public List<Item> itemsInFOV = new List<Item>();
        //public List<Item> DetectedItems { get { return itemsInFOV; } }

        void Start()
        {
            senseType = SenseType.Vision;
            StartCoroutine("DetectWithDelay", delay);
        }

        IEnumerator DetectWithDelay(float delay)
        {
            while (true)
            {
                DetectTargets();
                DetectAreas();

                yield return new WaitForSeconds(delay);
            }
        }

        /// <summary>
        /// Detects Objects with layer "TargetMask" and adds
        /// them to Visible Targets List.
        /// </summary>
        void DetectTargets()
        {
            // clear out the visible targets array
            visibleTargets.Clear();
            targetsInViewRadius.Clear();

            Collider[] agentsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

            for (int i = 0; i < agentsInViewRadius.Length; i++)
            {
                Transform agentTransform = agentsInViewRadius[i].transform;
                Vector3 dirToTarget = (agentTransform.position - transform.position).normalized;

                // targets in view radius ( the circular radius around agent)
                if(agentTransform != transform)
                {
                    targetsInViewRadius.Add(agentTransform); // ** //
                    visibleTargets.Add(agentTransform);
                }

                float distanceToTarget = Vector3.Distance(transform.position, agentTransform.position);
                if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
                {
                    if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
                    {
                        // visibleTargets.Add(agent);
                    }
                }
            }
        }


        void DetectAreas()
        {
            //foreach (var item in EnhancedNavigation.Instance.NavigationAreas)
            //{
            //    item.IsVisible(false);

            //    Vector3 dirToTarget = (item.GetCentre() - transform.position).normalized;
            //    float distanceToTarget = Vector3.Distance(transform.position, item.GetCentre());

            //    // if target is in view cone
            //    if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            //    {
            //        // if target is inside viewRadius
            //        if (distanceToTarget < viewRadius)
            //        {
            //            // if no obstacle is between target and area
            //            if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
            //            {
            //                item.IsVisible(true);
            //            }
            //        }
            //    }
            //}
        }

        //public void DetectItems()
        //{
        //    itemsInFOV.Clear();

        //    foreach (var item in ItemsManager.Instance.items)
        //    {
        //        Vector3 dirToTarget = (item.transform.position - transform.position).normalized;
        //        float distanceToTarget = Vector3.Distance(transform.position, item.transform.position);

        //        // if target is in view cone
        //        if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
        //        {
        //            // if target is inside viewRadius
        //            if (distanceToTarget < viewRadius)
        //            {
        //                Debug.DrawRay(transform.position, dirToTarget.normalized * 5, Color.red);

        //                // if no obstacle is between target and area
        //                if (!Physics.Raycast(transform.position, dirToTarget, distanceToTarget, obstacleMask))
        //                {
        //                    itemsInFOV.Add(item);
        //                }
        //            }
        //        }
        //    }
        //}

        //public Item ContainsItem(ItemType itemType)
        //{
        //    foreach (var item in DetectedItems)
        //    {
        //        if (item.itemType == itemType) { return item; }
        //    }
        //    return null;
        //}

        public override bool HandleMessage(Telegram msg)
        {
            throw new NotImplementedException();
        }
    }
}
