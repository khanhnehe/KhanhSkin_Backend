using System;
using System.ComponentModel;
using System.Reflection;

namespace KhanhSkin_BackEnd.Helper
{
    public static class EnumExtension
    {
        // Phương thức lấy giá trị mô tả (description) từ một giá trị enum
        public static string GetDescription(this Enum enumValue)
        {
            // Nếu giá trị enum là null, trả về chuỗi rỗng
            if (enumValue == null) return string.Empty;

            // Lấy thông tin về trường (field) của giá trị enum
            var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());

            // Lấy tất cả các thuộc tính DescriptionAttribute gắn với trường đó
            var descriptionAttributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

            // Nếu tồn tại thuộc tính mô tả, trả về giá trị mô tả đó, ngược lại trả về tên của enum
            return descriptionAttributes.Length > 0 ? descriptionAttributes[0].Description : enumValue.ToString();
        }

        // Phương thức để lấy giá trị enum dựa trên mô tả (description)
        public static T GetValueFromDescription<T>(string description) where T : Enum
        {
            // Duyệt qua tất cả các trường (field) trong kiểu enum
            foreach (var field in typeof(T).GetFields())
            {
                // Kiểm tra nếu trường đó có thuộc tính mô tả (DescriptionAttribute)
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                {
                    // Nếu mô tả của thuộc tính trùng với mô tả cần tìm, trả về giá trị enum tương ứng
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                }
                else
                {
                    // Nếu không có mô tả, kiểm tra tên trường có trùng với mô tả không
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }

            // Nếu không tìm thấy, trả về giá trị mặc định của enum (thường là giá trị đầu tiên)
            return default;
        }
    }
}
