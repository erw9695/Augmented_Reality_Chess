// Ethan Wong
// CS-UY 3943 VR/AR, Fall 2023
// Main.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(ARTrackedImageManager))]
public class Main : MonoBehaviour
{
    // There are three sets of different chess pieces that the user can choose from.
    // Import all of them at the start.
    public GameObject[] setA,setB,setC;
    // Contains all of the chess pieces for the selected set.
    private Dictionary<string, GameObject> prefabs = new Dictionary<string,GameObject>();
    public ARTrackedImageManager trackedImageManager;
    // Buttons that allow user to select a different set.
    public Button setAButton,setBButton,setCButton;
    // Image of the UI buttons (needed to change style on selection).
    private Image aImage,bImage,cImage; 
    // References to button text (needed to change style on selection).
    public TMP_Text buttonAText,buttonBText,buttonCText;
    // Color of the selected button.
    private Color setColor = new Color(205/255f,213/255f,178/255f);
    // Reference to dropdown with list of color options.
    public TMP_Dropdown colorLst;

    public void Awake() {
        // Get button images, used for styling.
        aImage = setAButton.GetComponent<Image>();
        bImage = setBButton.GetComponent<Image>();
        cImage = setCButton.GetComponent<Image>();
        // Add listeners to each button and dropdown.
        setAButton.onClick.AddListener(() => buttonClicked("Standard"));
        setBButton.onClick.AddListener(() => buttonClicked("LowPoly"));
        setCButton.onClick.AddListener(() => buttonClicked("Stone"));
        colorLst.onValueChanged.AddListener(delegate {dropdownChanged();});
        // By default load set A (standard piece set).
        loadObjects(setA);        
    }

    // Loads selected set into prefabs dictionary.
    public void loadObjects(GameObject[] set) {
        foreach (GameObject obj in set) {
            GameObject newObj = Instantiate(obj,Vector3.zero,Quaternion.identity);
            newObj.name = obj.name;
            prefabs.Add(newObj.name,newObj);
        }
    }

    // Change button style if selected.
    // Image "click" and TMP_Text "clickText" represent the selected button, all others are the other buttons.
    public void buttonStyleChange(Image click,Image other1,Image other2,TMP_Text clickText,TMP_Text other1Text,TMP_Text other2Text) {
        click.color = setColor;
        other1.color = Color.white;
        other2.color = Color.white;
        clickText.fontStyle = FontStyles.Bold;
        other1Text.fontStyle = FontStyles.Normal;
        other2Text.fontStyle = FontStyles.Normal;
    }

    // Clear all rendered pieces.
    public void resetPieces() {
        // Clear all loaded prefabs from the screen.
        foreach (GameObject val in prefabs.Values) {
           val.SetActive(false);
        }
        // Clear prefabs dictionary.
        prefabs.Clear();
    }

    // On button click, change button style and load set.
    public void buttonClicked(string button) {
        resetPieces();        

        if (button == "Standard") {
            colorLst.gameObject.SetActive(false);
            buttonStyleChange(aImage,bImage,cImage,buttonAText,buttonBText,buttonCText);
            loadObjects(setA);
        } else if (button == "LowPoly") {
            colorLst.gameObject.SetActive(true);
            buttonStyleChange(bImage,aImage,cImage,buttonBText,buttonAText,buttonCText);
            loadObjects(setB);
        } else {
            colorLst.gameObject.SetActive(false);
            buttonStyleChange(cImage,aImage,bImage,buttonCText,buttonAText,buttonBText);
            loadObjects(setC);
        }
    }

    // Change the color of the selected set.
    public void modifySetColor(GameObject[] set, Color colorA, Color colorB) {
        foreach (GameObject val in set) {
            string[] parts = val.name.Split("_");
            Renderer render = val.GetComponent<Renderer>();

            if (parts[1] == "A") {
                render.material.color = colorA;
            } else {
                render.material.color = colorB;
            }
        }
    }

    // Listener for dropdown, changes set color depending on the dropdown selection.
    // Because of how setA and setC prefabs are designed, its only possible to change the color of setB.
    public void dropdownChanged() {
        resetPieces();

        int colorSelection = colorLst.value;

        if (colorSelection == 0) {
            modifySetColor(setB,Color.black,Color.white);
        } else if (colorSelection == 1) {
            modifySetColor(setB,Color.red,Color.green);
        } else {
            modifySetColor(setB,Color.blue,Color.red);
        }

        loadObjects(setB);
    }

    public void OnEnable() {
        trackedImageManager.trackedImagesChanged += trackedMarkerChanged;
    }

    public void OnDisable() {
        trackedImageManager.trackedImagesChanged -= trackedMarkerChanged;
    }

    // Detects markers.
    public void trackedMarkerChanged(ARTrackedImagesChangedEventArgs eventArgs) {
        // If we started tracking a new marker ...
        foreach (ARTrackedImage img in eventArgs.added) {
            changeImage(img);
        }

        // Change in marker being tracked.
        foreach (ARTrackedImage img in eventArgs.updated) {
            if (img.trackingState == TrackingState.Tracking) {
                changeImage(img);
            } else {
                prefabs[img.referenceImage.name].SetActive(false);
            } 
        }

        // No longer tracking marker.
        foreach (ARTrackedImage img in eventArgs.removed) {
            prefabs[img.name].SetActive(false);
        }
    }

    // Rendering a piece ...
    public void changeImage(ARTrackedImage img) {
        // Get the marker's name, position, and rotation.
        string name = img.referenceImage.name;
        Vector3 pos = img.transform.position;
        Quaternion imgRot = img.transform.rotation;
        Quaternion rot = Quaternion.Euler(imgRot.eulerAngles.x,imgRot.eulerAngles.y,imgRot.eulerAngles.z);

        // Each marker's name corresponds to a prefab of the same name in each set.
        // Load that prefab, and give it the same position and rotation.
        GameObject objVal = prefabs[name];
        objVal.transform.position = pos;
        objVal.transform.rotation = rot;
        objVal.SetActive(true);
    }
}