// Thu ảnh khách chọn về JPEG cạnh dài tối đa `max` px trước khi gửi: ảnh điện thoại vài MB còn vài trăm KB, và vẽ lại
// qua canvas bỏ luôn EXIF (vị trí GPS, thông tin máy). Ảnh không đọc được thì ném lỗi.
export async function shrinkImage(file, max = 1600, quality = 0.85) {
  const bitmap = await createImageBitmap(file, { imageOrientation: 'from-image' })
  const scale = Math.min(1, max / Math.max(bitmap.width, bitmap.height))
  const canvas = document.createElement('canvas')
  canvas.width = Math.round(bitmap.width * scale)
  canvas.height = Math.round(bitmap.height * scale)
  const ctx = canvas.getContext('2d')
  ctx.fillStyle = '#fff'
  ctx.fillRect(0, 0, canvas.width, canvas.height)
  ctx.drawImage(bitmap, 0, 0, canvas.width, canvas.height)
  bitmap.close()
  const blob = await new Promise(resolve => canvas.toBlob(resolve, 'image/jpeg', quality))
  if (!blob) throw new Error('Không nén được ảnh')
  return blob
}
