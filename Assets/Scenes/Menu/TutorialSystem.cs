using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class TutorialSystem : MonoBehaviour
{
    [Header("UI Referencias")]
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Configuración")]
    [SerializeField] private float typewriterSpeed = 0.05f;
    [SerializeField] private bool autoAdvance = false;
    [SerializeField] private float autoAdvanceDelay = 3f;

    [System.Serializable]
    public class TutorialStep
    {
        [TextArea(3, 5)]
        public string message;
        public TutorialType type;
        public bool waitForInput;
        public float displayTime = 2f;
    }

    public enum TutorialType
    {
        SimpleMessage,
        WaitForMovement,
        WaitForAttack,
        WaitForSpecificKey
    }

    [Header("Pasos del Tutorial")]
    [SerializeField] private List<TutorialStep> tutorialSteps = new List<TutorialStep>();

    private int currentStep = 0;
    private bool isTyping = false;
    private bool stepCompleted = false;
    private bool tutorialActive = true;

    // Control de inputs
    private bool hasMovedWASD = false;
    private bool hasAttacked = false;

    [Header("Input Actions")]
    [SerializeField] private InputActionAsset inputActions;

    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction skipAction;

    private void Start()
    {
        SetupInputActions();
        InitializeTutorial();
        StartTutorial();
    }

    private void SetupInputActions()
    {
        // Si no se asigna un InputActionAsset, crear acciones manualmente
        if (inputActions == null)
        {
            // Crear acciones de input manualmente
            moveAction = new InputAction("Move", InputActionType.Value);

            // Crear un composite binding para WASD
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");

            attackAction = new InputAction("Attack", InputActionType.Button, "<Mouse>/leftButton");
            skipAction = new InputAction("Skip", InputActionType.Button, "<Keyboard>/escape");
        }
        else
        {
            // Usar las acciones del InputActionAsset
            moveAction = inputActions.FindAction("Move");
            attackAction = inputActions.FindAction("Attack");
            skipAction = inputActions.FindAction("Skip");
        }

        // Habilitar las acciones
        moveAction?.Enable();
        attackAction?.Enable();
        skipAction?.Enable();

        // Suscribirse a los eventos
        if (attackAction != null)
            attackAction.performed += OnAttackPerformed;

        if (skipAction != null)
            skipAction.performed += OnSkipPerformed;
    }

    private void InitializeTutorial()
    {
        // Configurar los pasos del tutorial por defecto si la lista está vacía
        if (tutorialSteps.Count == 0)
        {
            tutorialSteps.Add(new TutorialStep
            {
                message = "¡Bienvenido al tutorial!",
                type = TutorialType.SimpleMessage,
                waitForInput = false,
                displayTime = 2f
            });

            tutorialSteps.Add(new TutorialStep
            {
                message = "Usa las teclas WASD para moverte por el mundo.",
                type = TutorialType.WaitForMovement,
                waitForInput = true
            });

            tutorialSteps.Add(new TutorialStep
            {
                message = "¡Muy bien! Ahora haz click izquierdo para atacar.",
                type = TutorialType.WaitForAttack,
                waitForInput = true
            });

            tutorialSteps.Add(new TutorialStep
            {
                message = "¡Perfecto! Ya conoces los controles básicos.",
                type = TutorialType.SimpleMessage,
                waitForInput = false,
                displayTime = 3f
            });

            tutorialSteps.Add(new TutorialStep
            {
                message = "¡Tutorial completado! ¡Diviértete jugando!",
                type = TutorialType.SimpleMessage,
                waitForInput = false,
                displayTime = 2f
            });
        }
    }

    private void Update()
    {
        if (!tutorialActive || currentStep >= tutorialSteps.Count)
            return;

        CheckStepCompletion();
    }

    private void OnDestroy()
    {
        // Limpiar las suscripciones de eventos
        if (attackAction != null)
            attackAction.performed -= OnAttackPerformed;

        if (skipAction != null)
            skipAction.performed -= OnSkipPerformed;

        // Deshabilitar las acciones
        moveAction?.Disable();
        attackAction?.Disable();
        skipAction?.Disable();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        hasAttacked = true;
    }

    private void OnSkipPerformed(InputAction.CallbackContext context)
    {
        SkipTutorial();
    }

    private void StartTutorial()
    {
        if (tutorialSteps.Count > 0)
        {
            ShowStep(currentStep);
        }
    }

    private void ShowStep(int stepIndex)
    {
        if (stepIndex >= tutorialSteps.Count)
        {
            EndTutorial();
            return;
        }

        TutorialStep step = tutorialSteps[stepIndex];
        stepCompleted = false;

        // Resetear flags al entrar en pasos que esperan input
        if (step.type == TutorialType.WaitForMovement)
            hasMovedWASD = false;
        if (step.type == TutorialType.WaitForAttack)
            hasAttacked = false;

        // Mostrar el mensaje con efecto typewriter
        StartCoroutine(TypewriterEffect(step.message));

        // Auto avance para mensajes simples
        if (!step.waitForInput && step.type == TutorialType.SimpleMessage)
        {
            StartCoroutine(AutoAdvanceStep(step.displayTime));
        }
    }


    private IEnumerator TypewriterEffect(string message)
    {
        isTyping = true;
        tutorialText.text = "";

        foreach (char c in message)
        {
            tutorialText.text += c;
            yield return new WaitForSeconds(typewriterSpeed);
        }

        isTyping = false;
    }

    private IEnumerator AutoAdvanceStep(float delay)
    {
        // Esperar a que termine el typewriter antes de contar el delay
        yield return new WaitUntil(() => !isTyping);
        yield return new WaitForSeconds(delay);

        if (!stepCompleted)
        {
            NextStep();
        }
    }


    private void CheckStepCompletion()
    {
        if (stepCompleted || currentStep >= tutorialSteps.Count)
            return;

        TutorialStep currentTutorialStep = tutorialSteps[currentStep];

        switch (currentTutorialStep.type)
        {
            case TutorialType.WaitForMovement:
                CheckMovementInput();
                if (hasMovedWASD)
                {
                    stepCompleted = true;
                    StartCoroutine(DelayedNextStep(1f));
                }
                break;

            case TutorialType.WaitForAttack:
                CheckAttackInput();
                if (hasAttacked)
                {
                    stepCompleted = true;
                    StartCoroutine(DelayedNextStep(1f));
                }
                break;
        }
    }

    private void CheckMovementInput()
    {
        if (moveAction != null && moveAction.ReadValue<Vector2>().magnitude > 0.1f)
        {
            hasMovedWASD = true;
        }
    }

    private void CheckAttackInput()
    {
        // El ataque se maneja en OnAttackPerformed
    }

    private IEnumerator DelayedNextStep(float delay)
    {
        // Esperar a que termine el typewriter antes de avanzar
        yield return new WaitForSeconds(delay);
        yield return new WaitUntil(() => !isTyping);

        NextStep();
    }



    public void NextStep()
    {
        // OJO: ya hemos esperado a que acabe el typewriter en las corutinas,
        // así que aquí ya no bloqueamos el avance.
        if (isTyping)
            return;

        currentStep++;

        if (currentStep < tutorialSteps.Count)
        {
            ShowStep(currentStep);
        }
        else
        {
            EndTutorial();
        }
    }


    public void SkipTutorial()
    {
        StopAllCoroutines();
        EndTutorial();
    }

    private void EndTutorial()
    {
        tutorialActive = false;

        // Limpiar el texto del tutorial
        if (tutorialText != null)
            tutorialText.text = "";

        // Aquí puedes añadir lógica adicional como activar el gameplay normal
        Debug.Log("Tutorial completado!");

        // Opcional: Cargar la siguiente escena o activar el juego principal
        // SceneManager.LoadScene("MainGame");
    }

    // Método público para reiniciar el tutorial
    public void RestartTutorial()
    {
        currentStep = 0;
        hasMovedWASD = false;
        hasAttacked = false;
        stepCompleted = false;
        tutorialActive = true;

        SetupInputActions();
        StartTutorial();
    }

    // Método para añadir pasos dinámicamente
    public void AddTutorialStep(string message, TutorialType type, bool waitForInput = false, float displayTime = 2f)
    {
        TutorialStep newStep = new TutorialStep
        {
            message = message,
            type = type,
            waitForInput = waitForInput,
            displayTime = displayTime
        };

        tutorialSteps.Add(newStep);
    }
}