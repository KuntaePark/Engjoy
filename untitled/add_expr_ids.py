import json
import os

input_file_name = 'wordInfo.json'
output_file_name = 'wordInfo_with_ids.json' # 안전을 위해 새 파일로 저장

# 스크립트가 있는 디렉토리 경로 가져오기
script_dir = os.path.dirname(__file__)
input_file_path = os.path.join(script_dir, input_file_name)
output_file_path = os.path.join(script_dir, output_file_name)

try:
    # JSON 파일 읽기
    with open(input_file_path, 'r', encoding='utf-8') as f:
        data = json.load(f)

    # JSON 데이터가 리스트(배열)인지 확인
    if not isinstance(data, list):
        print(f"오류: '{input_file_name}'의 JSON 데이터가 리스트 형태가 아닙니다. 객체 배열인지 확인해주세요.")
        exit()

    # 각 항목에 exprId 추가
    for i, item in enumerate(data):
        # exprId를 1부터 순차적으로 할당
        # 만약 exprId가 이미 존재하거나 특정 패턴이 있다면, 더 정교한 ID 생성 로직이 필요할 수 있습니다.
        if 'exprId' not in item: # exprId가 아직 없는 경우에만 추가
            item['exprId'] = i + 1
        else:
            print(f"경고: 항목 {i}에 'exprId'가 이미 존재합니다. 기존 값 유지: {item['exprId']}")

    # 수정된 데이터를 새 JSON 파일에 쓰기
    with open(output_file_path, 'w', encoding='utf-8') as f:
        # indent=2는 JSON을 들여쓰기하여 가독성을 높여줍니다.
        # ensure_ascii=False는 한글이 유니코드 이스케이프 문자(\uXXXX)로 변환되지 않고 그대로 저장되게 합니다.
        json.dump(data, f, indent=2, ensure_ascii=False)

    print(f"'{input_file_name}'의 {len(data)}개 항목에 'exprId'가 성공적으로 추가/업데이트되었습니다.")
    print(f"결과 파일: {output_file_path}")

except FileNotFoundError:
    print(f"오류: '{input_file_name}' 파일을 찾을 수 없습니다: {input_file_path}")
except json.JSONDecodeError as e:
    print(f"오류: '{input_file_name}' 파일의 JSON 디코딩에 실패했습니다: {e}")
except Exception as e:
    print(f"예상치 못한 오류가 발생했습니다: {e}")