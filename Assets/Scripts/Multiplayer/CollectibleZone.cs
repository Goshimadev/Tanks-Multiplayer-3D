﻿/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using Photon.Pun;
using static TanksMP.MultGameManager;

namespace TanksMP
{
    /// <summary>
    /// Component that acts as an area to trigger collection of CollectibleTeam items.
    /// E.g. necessary for team bases in Capture The Flag mode. Needs a Collider to trigger.
    /// </summary>
	public class CollectibleZone : MonoBehaviour
    {
        /// <summary>
        /// Team index this zone belongs to.
        /// Teams are defined in the GameManager script inspector.
        /// </summary>
        public int teamIndex = 0;

        /// <summary>
        /// Optional: Other Collectible, that needs to be at its spawn position for this zone to
        /// trigger a successful collection. One example would be for Capture The Flag, where the
        /// red flags needs to be at the red spawn, in order to successfully collect the blue flag.
        /// </summary>
        public ObjectSpawner requireObject;

        /// <summary>
        /// Clip to play when a CollectibleTeam item is brought to this zone.
        /// </summary>
        public AudioClip scoreClip;


        /// <summary>
        /// Server only: check for collectibles colliding with the zone.
        /// Possible collision are defined in the Physics Matrix. 
        /// </summary>
        public void OnTriggerEnter(Collider col)
        {
            if (!PhotonNetwork.IsMasterClient)
                return;

            //the game is already over so don't do anything
            if (MultGameManager.GetInstance().IsGameOver()) return;

            //check for the required object
            //continue, if it is not assigned to begin with
            if (requireObject != null)
            {
                //the required object is not instantiated
                if (requireObject.obj == null)
                    return;

                //the required object either does not have a CollectibleTeam component,
                //is still being carried around or not yet at back at the spawn position
                CollectibleTeam colReq = requireObject.obj.GetComponent<CollectibleTeam>();
                if (colReq == null || colReq.carrierId >= 0 ||
                    colReq.transform.position != requireObject.transform.position)
                    return;
            }

            CollectibleTeam colOther = col.gameObject.GetComponent<CollectibleTeam>();

            //a team item, which is not our own, has been brought to this zone 
            if (colOther != null && colOther.teamIndex != teamIndex)
            {
                //if (scoreClip) AudioManager.Play3D(scoreClip, transform.position);

                //add points for this score type to the correct team
                MultGameManager.GetInstance().AddScore(ScoreType.Capture, teamIndex);
                //the maximum score has been reached now
                if (MultGameManager.GetInstance().IsGameOver())
                {
                    //close room for joining players
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    //tell all clients the winning team
                    MultGameManager.GetInstance().localPlayer.photonView.RPC("RpcGameOver", RpcTarget.All, (byte)teamIndex);
                    return;
                }

                //remove network messages about the Collectible since it is about to get destroyed 
                PhotonNetwork.RemoveRPCs(colOther.spawner.photonView);
                colOther.spawner.photonView.RPC("Destroy", RpcTarget.All);
            }
        }
    }
}