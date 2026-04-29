using UnityEngine;

public interface IPossessable
{
    void OnPossess(Transform playerTransform);
    void OnUnpossess();

    // เปลี่ยนพารามิเตอร์จาก speedMultiplier เป็นการเช็คว่า "กดวิ่งอยู่หรือไม่"
    void Move(Vector3 moveDir, bool isSprinting);
    void Action();

    // ฟังก์ชันสำหรับระบบ Stamina และ Stress
    float GetStaminaPercentage();
    float GetStressPercentage();
    void AddStress(float amount); // เอาไว้ให้ศัตรูเรียกใช้เวลาตรวจเจอ
}