using UnityEngine;
using System.Collections;

public class InteractableDoor : MonoBehaviour
{
    [Header("Настройки анимации")]
    [SerializeField] private GameObject doorModel; // Объект самой двери, который будет двигаться
    [SerializeField] private float openHeight = 3.0f; // На сколько юнитов дверь поднимется
    [SerializeField] private float openSpeed = 2.0f; // Скорость открытия

    [Header("Настройки взаимодействия")]
    [SerializeField] private KeyCode interactKey = KeyCode.F; // Кнопка взаимодействия

    private bool isPlayerInRange = false;
    private bool isOpening = false;
    private bool isOpen = false;
    private Vector3 closedPosition;

    void Start()
    {
        if (doorModel == null)
        {
            Debug.LogError($"На объекте {gameObject.name} не назначена модель двери в скрипте InteractableDoor!");
            enabled = false;
            return;
        }
        // Запоминаем начальное (закрытое) положение модели
        closedPosition = doorModel.transform.localPosition;
    }

    void Update()
    {
        // Если игрок рядом, дверь не открывается и нажата кнопка F
        if (isPlayerInRange && !isOpening && !isOpen && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(OpenDoorRoutine());
        }
    }

    // Корутина для плавного открытия
    IEnumerator OpenDoorRoutine()
    {
        isOpening = true;
        Vector3 targetPosition = closedPosition + Vector3.up * openHeight;
        float elapsed = 0;

        // Пока не достигли целевой высоты
        while (elapsed < 1.0f)
        {
            // Плавное перемещение (Lerp)
            doorModel.transform.localPosition = Vector3.Lerp(closedPosition, targetPosition, elapsed);
            elapsed += Time.deltaTime * openSpeed;
            yield return null; // Ждем следующего кадра
        }

        // Устанавливаем точную финальную позицию
        doorModel.transform.localPosition = targetPosition;

        // Отключаем физический коллайдер двери (если он есть на модели), чтобы игрок мог пройти
        Collider doorCollider = doorModel.GetComponent<Collider>();
        if (doorCollider != null && !doorCollider.isTrigger)
        {
            doorCollider.enabled = false;
        }

        isOpen = true;
        isOpening = false;
    }

    // Обработка входа игрока в триггер
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Здесь можно включить UI-подсказку "Нажмите F чтобы открыть"
        }
    }

    // Обработка выхода игрока из триггера
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Здесь можно выключить UI-подсказку
        }
    }
}