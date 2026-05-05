using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(OpportunityData))]
public class OpportunityEditor : Editor
{
    public override void OnInspectorGUI()
    {
        OpportunityData data = (OpportunityData)target;

        data.title = EditorGUILayout.TextField("Название", data.title);
        data.description = EditorGUILayout.TextField("Описание", data.description);
        data.type = (OpportunityType)EditorGUILayout.EnumPopup("Тип", data.type);

        EditorGUILayout.Space();

        switch (data.type)
        {
            case OpportunityType.Job:
                data.jobIncomePerHour = EditorGUILayout.IntField("Доход за час", data.jobIncomePerHour);
                data.jobHours = EditorGUILayout.IntField("Часы работы", data.jobHours);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Переменный доход (шаг 50)");

                data.jobBonusMin = EditorGUILayout.IntField("Мин бонус", data.jobBonusMin);
                data.jobBonusMax = EditorGUILayout.IntField("Макс бонус", data.jobBonusMax);
                break;

            case OpportunityType.Business:
                data.businessCost = EditorGUILayout.IntField("Вложения", data.businessCost);
                data.businessIncome = EditorGUILayout.IntField("Доход", data.businessIncome);
                data.businessStartTime = EditorGUILayout.IntField("Время старта (периоды)", data.businessStartTime);
                data.businessTimeCost = EditorGUILayout.IntField("Затраты времени", data.businessTimeCost);
                break;

            case OpportunityType.Invest:
                data.investCost = EditorGUILayout.IntField("Цена за акцию", data.investCost);
                data.investRisk = EditorGUILayout.IntSlider("Риск", data.investRisk, 1, 5);
                break;

            case OpportunityType.Realty:
                data.realtyCost = EditorGUILayout.IntField("Стоимость", data.realtyCost);
                data.realtyIncome = EditorGUILayout.IntField("Доход", data.realtyIncome);
                data.realtyTimeCost = EditorGUILayout.IntField("Затраты времени", data.realtyTimeCost);
                break;
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(data);
        }
    }
}
