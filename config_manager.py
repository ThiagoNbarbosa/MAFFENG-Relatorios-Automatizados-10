import json
import os
from datetime import datetime

class ConfigManager:
    """Gerenciador de configurações para placeholders do sistema"""
    
    def __init__(self, config_file='config/placeholders_config.json'):
        self.config_file = config_file
        self.ensure_config_dir()
        self.default_config = {
            # Configuração padrão dos placeholders
            'placeholders': {
                'numero_contrato': {
                    'label': 'Número do Contrato',
                    'type': 'variable',  # 'fixed' ou 'variable'
                    'value': '',
                    'required': True,
                    'section': 'projeto'
                },
                'ordem_servico': {
                    'label': 'Ordem de Serviço',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'projeto'
                },
                'data_elaboracao': {
                    'label': 'Data de Elaboração',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'projeto'
                },
                'data_atendimento': {
                    'label': 'Data de Atendimento',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'projeto'
                },
                'prefixo_agencia': {
                    'label': 'Prefixo da Agência',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'dependencia'
                },
                'nome_dependencia': {
                    'label': 'Nome da Dependência',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'dependencia'
                },
                'endereco_completo': {
                    'label': 'Endereço Completo',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'dependencia'
                },
                'uf': {
                    'label': 'UF',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'dependencia'
                },
                'tipo_atendimento': {
                    'label': 'Tipo de Atendimento',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'dependencia'
                },
                'responsavel_dependencia': {
                    'label': 'Responsável da Dependência',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'responsaveis'
                },
                'responsavel_tecnico': {
                    'label': 'Responsável Técnico',
                    'type': 'variable',
                    'value': '',
                    'required': True,
                    'section': 'responsaveis'
                },
                'elaborado_por': {
                    'label': 'Elaborado por',
                    'type': 'fixed',
                    'value': 'Ygor Augusto Fernandes',
                    'required': True,
                    'section': 'empresa'
                },
                'empresa': {
                    'label': 'Empresa',
                    'type': 'fixed',
                    'value': 'MAFFENG - Engenharia e Manutenção Profissional',
                    'required': True,
                    'section': 'empresa'
                }
            },
            'sections': {
                'projeto': 'Informações do Projeto',
                'dependencia': 'Informações da Dependência',
                'responsaveis': 'Responsáveis',
                'empresa': 'Informações da Empresa'
            },
            'updated_at': datetime.now().isoformat()
        }
        self.load_config()
    
    def ensure_config_dir(self):
        """Garante que o diretório de configuração existe"""
        config_dir = os.path.dirname(self.config_file)
        if config_dir:
            os.makedirs(config_dir, exist_ok=True)
    
    def load_config(self):
        """Carrega configuração do arquivo ou cria padrão"""
        try:
            if os.path.exists(self.config_file):
                with open(self.config_file, 'r', encoding='utf-8') as f:
                    self.config = json.load(f)
                # Merge com configuração padrão para novos campos
                self._merge_with_default()
            else:
                self.config = self.default_config.copy()
                self.save_config()
        except Exception as e:
            print(f"Erro ao carregar configuração: {e}")
            self.config = self.default_config.copy()
    
    def _merge_with_default(self):
        """Mescla configuração existente com novos campos padrão"""
        for key, value in self.default_config['placeholders'].items():
            if key not in self.config['placeholders']:
                self.config['placeholders'][key] = value
        
        # Atualiza seções se necessário
        self.config['sections'] = self.default_config['sections']
    
    def save_config(self):
        """Salva configuração no arquivo"""
        try:
            self.config['updated_at'] = datetime.now().isoformat()
            with open(self.config_file, 'w', encoding='utf-8') as f:
                json.dump(self.config, f, indent=2, ensure_ascii=False)
            return True
        except Exception as e:
            print(f"Erro ao salvar configuração: {e}")
            return False
    
    def get_placeholder_config(self, placeholder_key):
        """Retorna configuração de um placeholder específico"""
        return self.config['placeholders'].get(placeholder_key, {})
    
    def update_placeholder(self, placeholder_key, field_type, value, label=None):
        """Atualiza configuração de um placeholder"""
        if placeholder_key in self.config['placeholders']:
            self.config['placeholders'][placeholder_key]['type'] = field_type
            self.config['placeholders'][placeholder_key]['value'] = value
            if label:
                self.config['placeholders'][placeholder_key]['label'] = label
            return self.save_config()
        return False
    
    def get_variable_fields(self):
        """Retorna apenas campos configurados como variáveis"""
        return {
            key: config for key, config in self.config['placeholders'].items()
            if config.get('type') == 'variable'
        }
    
    def get_fixed_fields(self):
        """Retorna apenas campos configurados como fixos"""
        return {
            key: config for key, config in self.config['placeholders'].items()
            if config.get('type') == 'fixed'
        }
    
    def get_all_placeholders(self):
        """Retorna todos os placeholders organizados por seção"""
        organized = {}
        for section_key, section_name in self.config['sections'].items():
            organized[section_key] = {
                'name': section_name,
                'placeholders': {}
            }
        
        for key, config in self.config['placeholders'].items():
            section = config.get('section', 'outros')
            if section not in organized:
                organized[section] = {
                    'name': section.title(),
                    'placeholders': {}
                }
            organized[section]['placeholders'][key] = config
        
        return organized
    
    def get_form_data_with_defaults(self, form_data):
        """Mescla dados do formulário com valores fixos configurados"""
        result = form_data.copy()
        
        # Adiciona valores fixos
        for key, config in self.get_fixed_fields().items():
            result[key] = config['value']
        
        return result

# Instância global do gerenciador de configuração
config_manager = ConfigManager()