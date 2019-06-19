//-----------------------------------------------------------------------
// <copyright file="HelloARController.cs" company="Google">
//
// Copyright 2017 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.HelloAR
{
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.UI;
    using System.Collections;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    /// <summary>
    /// Controls the HelloAR example.
    /// </summary>
    public class HelloARController : MonoBehaviour
    {
        /// <summary>
        /// The first-person camera being used to render the passthrough camera image (i.e. AR background).
        /// </summary>
        public Camera FirstPersonCamera;

        /// <summary>
        /// A prefab for tracking and visualizing detected planes.
        /// </summary>
        public GameObject DetectedPlanePrefab;

        /// <summary>
        /// A model to place when a raycast from a user touch hits a plane.
        /// </summary>
        public GameObject stool_Prefab;
        public GameObject table_Prefab;
        public GameObject chair_Prefab;
        private bool firstBtnClicked = true;
        private bool secondBtnClicked = false;
        private bool thirdBtnClicked = false;
        /// <summary>
        /// A model to place when a raycast from a user touch hits a feature point.
        /// </summary>

        /// <summary>
        /// A gameobject parenting UI for displaying the "searching for planes" snackbar.
        /// </summary>
        public GameObject SearchingForPlaneUI;
        public GameObject SearchingForTapUI;
        /// <summary>
        /// The rotation in degrees need to apply to model when the Andy model is placed.
        /// </summary>
        private const float k_ModelRotation = 180.0f;

        /// A list to hold new planes ARCore began tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        /// </summary>
        private List<DetectedPlane> m_NewPlanes = new List<DetectedPlane>();

        /// <summary>
        /// A list to hold all planes ARCore is tracking in the current frame. This object is used across
        /// the application to avoid per-frame allocations.
        /// </summary>
        private List<DetectedPlane> m_AllPlanes = new List<DetectedPlane>();
        
        /// <summary>
        /// True if the app is in the process of quitting due to an ARCore connection error, otherwise false.
        /// </summary>
        private bool m_IsQuitting = false;     
        public int numberOfObjectsAllowed = 1;
        private int currentNumberOfObjects = 0;

        //GameObject prefab;

        

        public void firstButtonClick()
        {
            GameObject parentanchor = GameObject.Find("Anchor/myobject").transform.parent.gameObject;
            Destroy(parentanchor);
            currentNumberOfObjects = 0;
            firstBtnClicked = true;
            secondBtnClicked = false;
            thirdBtnClicked = false;
        }

        public void secondButtonClick()
        {
            GameObject parentanchor = GameObject.Find("Anchor/myobject").transform.parent.gameObject;
            Destroy(parentanchor);
            currentNumberOfObjects = 0;
            secondBtnClicked = true;
            firstBtnClicked = false;
            thirdBtnClicked = false;
        }
        public void thirdButtonClick()
        {
            GameObject parentanchor = GameObject.Find("Anchor/myobject").transform.parent.gameObject;
            Destroy(parentanchor);
            currentNumberOfObjects = 0;
            thirdBtnClicked = true;
            firstBtnClicked = false;
            secondBtnClicked = false;
        }
        /// <summary>
        /// The Unity Update() method.
        /// </summary>
        public void Update()
        {
            // _UpdateApplicationLifecycle();

            // Hide snackbar when currently tracking at least one plane.
            Session.GetTrackables<DetectedPlane>(m_AllPlanes);
            bool showSearchingUI = true;
            bool showTapUI = false;
            for (int i = 0; i < m_AllPlanes.Count; i++)
            {
                if (m_AllPlanes[i].TrackingState == TrackingState.Tracking)
                {
                    showSearchingUI = false;
                    showTapUI = true;
                    break;
                }
            }

            SearchingForPlaneUI.SetActive(showSearchingUI);

            //tap to display when surface detected 
            if ((GameObject.Find("Anchor/myobject") != null))
            {
                showTapUI = false;
            }

            SearchingForTapUI.SetActive(showTapUI);

            // If the player has not touched the screen, we are done with this update.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            TrackableHit hit;
            TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon |
                TrackableHitFlags.FeaturePointWithSurfaceNormal;

            if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
            {
                if (currentNumberOfObjects < numberOfObjectsAllowed)
                {
                   
                    // Use hit pose and camera pose to check if hittest is from the
                    // back of the plane, if it is, no need to create the anchor.
                    if ((hit.Trackable is DetectedPlane) &&
                    Vector3.Dot(FirstPersonCamera.transform.position - hit.Pose.position,
                        hit.Pose.rotation * Vector3.up) < 0)
                    {
                        Debug.Log("Hit at back of the current DetectedPlane");
                    }
                    else
                    {
                        if (GameObject.Find("Anchor/myobject") != null)
                        {
                            GameObject parentanchor = GameObject.Find("Anchor/myobject").transform.parent.gameObject;
                            Destroy(parentanchor);
                        }

                        GameObject obj = null;
                        if (firstBtnClicked == true)
                        {
                            // Instantiate first Asset model at the hit pose.
                            obj = Instantiate(table_Prefab, hit.Pose.position, hit.Pose.rotation);
                            currentNumberOfObjects = currentNumberOfObjects + 1;

                        }
                        else if (secondBtnClicked == true)
                        {
                            // Second button clicked: Instantiate second Asset model at the hit pose.
                            obj = Instantiate(stool_Prefab, hit.Pose.position, hit.Pose.rotation);
                            currentNumberOfObjects = currentNumberOfObjects + 1;

                        }
                        else if (thirdBtnClicked == true)
                        {
                            // Second button clicked: Instantiate second Asset model at the hit pose.
                            obj = Instantiate(chair_Prefab, hit.Pose.position, hit.Pose.rotation);
                            currentNumberOfObjects = currentNumberOfObjects + 1;

                        }
                        obj.transform.Rotate(0, k_ModelRotation, 0, Space.Self);
                        obj.name = "myobject";
                        // Create an anchor to allow ARCore to track the hitpoint as understanding of the physical
                        // world evolves.
                        var anchor = hit.Trackable.CreateAnchor(hit.Pose);

                        // Make the object model a child of the anchor.
                        obj.transform.parent = anchor.transform;
                    }

                }
            }

        }

    }
}

