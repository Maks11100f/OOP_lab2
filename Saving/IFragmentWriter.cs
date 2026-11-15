using OOP_lab2.Models;

namespace OOP_lab2.Saving;

/// <summary>
/// Продукт шаблону «Фабричний метод»: об'єкт, що записує відфільтрований
/// фрагмент розкладу у вже відкритий файл у певному форматі.
/// </summary>
public interface IFragmentWriter
{
    void Write(Stream stream, IReadOnlyList<Lesson> lessons);
}
