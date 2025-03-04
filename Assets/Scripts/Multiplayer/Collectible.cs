﻿/*  This file is part of the "Tanks Multiplayer" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using Photon.Pun;

namespace TanksMP
{
    /// <summary>
    /// Base class for all derived Collectibles (health, shields, etc.) consumed or carried around.
    /// Extend this to create highly customized Collectible with specific functionality.
    /// </summary>
	public class Collectible : MonoBehaviour
	{	    
        /// <summary>
        /// Clip to play when this Collectible is consumed by a player.
        /// </summary>
        public AudioClip useClip;

        /// <summary>
        /// Reference to the local object (script) that spawned this Collectible.
        /// </summary>
        [HideInInspector]
        public ObjectSpawner spawner;

        /// <summary>
        /// Persistent network (PhotonView) ID of the Player that picked up this Collectible.
        /// </summary>
        [HideInInspector]
        public int carrierId = -1;
        
                  
        /// <summary>
        /// Server only: check for players colliding with this item.
        /// Possible collision are defined in the Physics Matrix.
        /// </summary>
        public virtual void OnTriggerEnter(Collider col)
		{
            if (!PhotonNetwork.IsMasterClient)
                return;
            
    		GameObject obj = col.gameObject;
			Player player = obj.GetComponent<Player>();

            //try to apply collectible to player, the result should be true
            if (Apply(player))
            {
                //destroy after use
                spawner.photonView.RPC("Destroy", RpcTarget.All);           
            }
		}


        /// <summary>
        /// Tries to apply the Collectible to a colliding player. Returns 'true' if consumed.
        /// Override this method in your own Collectible script to implement custom behavior.
        /// </summary>
        public virtual bool Apply(Player p)
		{
            //do something to the player
            if (p == null)
                return false;
            else
                return true;
		}


        /// <summary>
        /// Virtual implementation called when this Collectible gets picked up.
        /// This is called for CollectionType = Pickup items only.
        /// </summary>
        public virtual void OnPickup()
        {
        }


        /// <summary>
        /// Virtual implementation called when this Collectible gets dropped on player death.
        /// This is called for CollectionType = Pickup items only.
        /// </summary>
        public virtual void OnDrop()
        {
        }


        /// <summary>
        /// Virtual implementation called when this Collectible gets returned.
        /// This is called for CollectionType = Pickup items only.
        /// </summary>
        public virtual void OnReturn()
        {
        }


        //if consumed, play audio clip. Now with the Collectible despawned,
        //set the next spawn time on the managing ObjectSpawner script
        void OnDespawn()
        {
        //    if (useClip) AudioManager.Play3D(useClip, transform.position);
            carrierId = -1;
            spawner.SetRespawn();
        }
    }
}
