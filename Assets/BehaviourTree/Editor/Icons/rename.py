import os

dirname = "D:\mate-unity-android\Assets\MTest\BTree\Editor\Icons"

def rename_files(folder_path):
    # 遍历文件夹下的所有文件
    for file_name in os.listdir(folder_path):
        file_path = os.path.join(folder_path, file_name)
        # 判断是否为文件
        if os.path.isfile(file_path):
            # 获取文件扩展名
            extension = os.path.splitext(file_path)[1]
            # 判断是否为 .png 结尾
            if extension == '.png':
                new_file_name = file_name.replace('Dark','')[0:-8]+'.png'
                new_file_path = os.path.join(folder_path, new_file_name)
                # 重命名文件 
                os.rename(file_path, new_file_path)

rename_files(dirname) 