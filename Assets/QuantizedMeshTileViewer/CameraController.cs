﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform _target;
    public float Speed = 2;
    public GameObject[] anchors;
    public int _currentAnchorId = 0;
    bool isAnimating = false;

    /// <summary>
    /// Normal speed of camera movement.
    /// </summary>
    public float movementSpeed = 10f;

    /// <summary>
    /// Speed of camera movement when shift is held down,
    /// </summary>
    public float fastMovementSpeed = 100f;

    /// <summary>
    /// Sensitivity for free look.
    /// </summary>
    public float freeLookSensitivity = 3f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel.
    /// </summary>
    public float zoomSensitivity = 10f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel (fast mode).
    /// </summary>
    public float fastZoomSensitivity = 50f;

    /// <summary>
    /// Set to true when free looking (on right mouse button).
    /// </summary>
    private bool looking = false;

    GameObject currentlySelected;

    void Start()
    {
        _target = anchors[0].transform;
    }

    public void AnimateTo(Transform target)
    {
        isAnimating = true;
        _target = target;
    }

    void Update()
    {
        Zoom(); //rotates between vantage points

        PanRotate();

        SelectObject();
        
    }

    private void SelectObject()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hitInfo = new RaycastHit();
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo) && hitInfo.transform.tag == "Hub")
            {
                if (currentlySelected != null) //clear selection
                {
                    currentlySelected.GetComponentInChildren<TMPro.TextMeshPro>().color = Color.black;
                    currentlySelected = null;
                }

                currentlySelected = hitInfo.transform.gameObject;
                currentlySelected.GetComponentInChildren<TMPro.TextMeshPro>().color = Color.green;
            }
            else
            {
                if (currentlySelected != null) //clear selection
                {
                    currentlySelected.GetComponentInChildren<TMPro.TextMeshPro>().color = Color.black;
                    currentlySelected = null;
                }
            }
        }
    }

    void OnDisable()
    {
        StopLooking();
    }

    /// <summary>
    /// Enable free looking.
    /// </summary>
    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Disable free looking.
    /// </summary>
    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void PanRotate()
    {
        if (!isAnimating)
        {
            var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            var movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                transform.position = transform.position + (-transform.right * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                transform.position = transform.position + (transform.right * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            {
                transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            {
                transform.position = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.Q))
            {
                transform.position = transform.position + (transform.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.E))
            {
                transform.position = transform.position + (-transform.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
            {
                transform.position = transform.position + (Vector3.up * movementSpeed * Time.deltaTime);
            }

            if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
            {
                transform.position = transform.position + (-Vector3.up * movementSpeed * Time.deltaTime);
            }

            if (looking)
            {
                float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
                float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
                transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
            }

            //float axis = Input.GetAxis("Mouse ScrollWheel");
            //if (axis != 0)
            //{
            //    var zoomSensitivity = fastMode ? this.fastZoomSensitivity : this.zoomSensitivity;
            //    transform.position = transform.position + transform.forward * axis * zoomSensitivity;
            //}

            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                StartLooking();
            }
            else if (Input.GetKeyUp(KeyCode.Mouse1))
            {
                StopLooking();
            }
        }
    }

    private void Zoom()
    {
        var d = Input.GetAxis("Mouse ScrollWheel");
        if (d > 0f)
        {
            // scroll up
            if (_currentAnchorId + 1 < anchors.Length)
            {
                _currentAnchorId++;
                AnimateTo(anchors[_currentAnchorId].transform);
            }
        }
        else if (d < 0f)
        {
            // scroll down
            if (_currentAnchorId - 1 >= 0)
            {
                _currentAnchorId--;
                AnimateTo(anchors[_currentAnchorId].transform);
            }
        }

        if (isAnimating)
        {
            transform.position = Vector3.Lerp(transform.position, _target.position, Time.deltaTime * Speed);
            transform.rotation = Quaternion.Lerp(transform.rotation, _target.rotation, Time.deltaTime * Speed);

            if (Vector3.Distance(transform.position, _target.position) < 0.1f)
            {
                isAnimating = false;
                transform.position = _target.position;
                transform.rotation = _target.rotation;
            }
        }

        
    }
}
